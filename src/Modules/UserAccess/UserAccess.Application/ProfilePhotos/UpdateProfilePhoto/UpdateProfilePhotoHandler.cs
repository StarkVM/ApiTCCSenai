using Microsoft.Extensions.Logging;
using UserAccess.Application.Abstractions;
using UserAccess.Application.ProfilePhotos.Records;
using UserAccess.Application.ProfilePhotos.UpdateProfilePhoto.Records;
using UserAccess.Domain.Interfaces;

namespace UserAccess.Application.ProfilePhotos.UpdateProfilePhoto;

/// <summary>
/// Handles user profile photo update.
/// / Manipula a atualização da foto de perfil do usuário.
/// </summary>
public sealed class UpdateProfilePhotoHandler
{
    private static readonly HashSet<string> AllowedContentTypes = new(
        StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/webp"
    };

    private const long MaxPhotoSizeInBytes = 5 * 1024 * 1024;

    private readonly IUserRepository _userRepository;
    private readonly IUserProfilePhotoStorage _profilePhotoStorage;
    private readonly IUserProfilePhotoUrlProvider _profilePhotoUrlProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly ILogger<UpdateProfilePhotoHandler> _logger;

    public UpdateProfilePhotoHandler(
        IUserRepository userRepository,
        IUserProfilePhotoStorage profilePhotoStorage,
        IUserProfilePhotoUrlProvider profilePhotoUrlProvider,
        IUnitOfWork unitOfWork,
        IClock clock,
        ILogger<UpdateProfilePhotoHandler> logger)
    {
        _userRepository = userRepository;
        _profilePhotoStorage = profilePhotoStorage;
        _profilePhotoUrlProvider = profilePhotoUrlProvider;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _logger = logger;
    }
    
    public async Task<ProfilePhotoResult> HandleAsync(
        UpdateProfilePhotoCommand command,
        CancellationToken cancellationToken)
    {
        ValidateCommand(command);

        var user = await _userRepository.GetByIdAsync(
            command.UserId,
            cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException("USER_NOT_FOUND");
        }

        string? uploadedStorageKey = null;

        try
        {
            uploadedStorageKey = await _profilePhotoStorage.UploadAsync(
                user.Id,
                command.Photo,
                cancellationToken);

            var nowUtc = _clock.UtcNow;

            var oldStorageKey = user.ReplaceProfilePhoto(
                uploadedStorageKey,
                nowUtc);

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(oldStorageKey))
            {
                await DeleteOldPhotoBestEffortAsync(
                    oldStorageKey,
                    cancellationToken);
            }

            var accessUrl = _profilePhotoUrlProvider.Generate(
                uploadedStorageKey);

            return new ProfilePhotoResult(
                user.Id,
                true,
                accessUrl.Url,
                accessUrl.ExpiresAtUtc,
                user.ProfilePhotoUpdatedAtUtc);
        }
        catch
        {
            if (!string.IsNullOrWhiteSpace(uploadedStorageKey))
            {
                await DeleteUploadedPhotoBestEffortAsync(
                    uploadedStorageKey,
                    cancellationToken);
            }

            throw;
        }
    }

    private static void ValidateCommand(
        UpdateProfilePhotoCommand command)
    {
        if (command.UserId == Guid.Empty)
        {
            throw new ArgumentException("USER_ID_REQUIRED");
        }

        if (command.Photo is null)
        {
            throw new ArgumentException("PROFILE_PHOTO_REQUIRED");
        }

        if (string.IsNullOrWhiteSpace(command.Photo.FileName))
        {
            throw new ArgumentException("PROFILE_PHOTO_FILE_NAME_REQUIRED");
        }

        if (string.IsNullOrWhiteSpace(command.Photo.ContentType))
        {
            throw new ArgumentException("PROFILE_PHOTO_CONTENT_TYPE_REQUIRED");
        }

        if (!AllowedContentTypes.Contains(command.Photo.ContentType))
        {
            throw new ArgumentException("PROFILE_PHOTO_CONTENT_TYPE_NOT_ALLOWED");
        }

        if (command.Photo.Length <= 0)
        {
            throw new ArgumentException("PROFILE_PHOTO_EMPTY");
        }

        if (command.Photo.Length > MaxPhotoSizeInBytes)
        {
            throw new ArgumentException("PROFILE_PHOTO_TOO_LARGE");
        }
    }

    private async Task DeleteOldPhotoBestEffortAsync(
        string storageKey,
        CancellationToken cancellationToken)
    {
        try
        {
            await _profilePhotoStorage.DeleteAsync(
                storageKey,
                cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to delete old profile photo. StorageKey: {StorageKey}",
                storageKey);
        }
    }

    private async Task DeleteUploadedPhotoBestEffortAsync(
        string storageKey,
        CancellationToken cancellationToken)
    {
        try
        {
            await _profilePhotoStorage.DeleteAsync(
                storageKey,
                cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to delete uploaded profile photo after failure. StorageKey: {StorageKey}",
                storageKey);
        }
    }
}