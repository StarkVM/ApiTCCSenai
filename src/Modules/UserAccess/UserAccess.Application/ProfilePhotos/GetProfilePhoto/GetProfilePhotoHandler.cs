using UserAccess.Application.Abstractions;
using UserAccess.Application.ProfilePhotos.GetProfilePhoto.Records;
using UserAccess.Application.ProfilePhotos.Records;
using UserAccess.Domain.Interfaces;

namespace UserAccess.Application.ProfilePhotos.GetProfilePhoto;

/// <summary>
/// Handles the user profile photo query.
/// / Manipula a consulta da foto de perfil do usuário.
/// </summary>
public sealed class GetProfilePhotoHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IUserProfilePhotoUrlProvider _profilePhotoUrlProvider;

    public GetProfilePhotoHandler(
        IUserRepository userRepository,
        IUserProfilePhotoUrlProvider profilePhotoUrlProvider)
    {
        _userRepository = userRepository;
        _profilePhotoUrlProvider = profilePhotoUrlProvider;
    }
    
    public async Task<ProfilePhotoResult> HandleAsync(
        GetProfilePhotoQuery query,
        CancellationToken cancellationToken)
    {
        if (query.UserId == Guid.Empty)
        {
            throw new ArgumentException("USER_ID_REQUIRED");
        }

        var user = await _userRepository.GetByIdAsync(
            query.UserId,
            cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException("USER_NOT_FOUND");
        }

        if (string.IsNullOrWhiteSpace(user.ProfilePhotoStorageKey))
        {
            return new ProfilePhotoResult(
                user.Id,
                false,
                null,
                null,
                null);
        }

        var accessUrl = _profilePhotoUrlProvider.Generate(
            user.ProfilePhotoStorageKey);

        return new ProfilePhotoResult(
            user.Id,
            true,
            accessUrl.Url,
            accessUrl.ExpiresAtUtc,
            user.ProfilePhotoUpdatedAtUtc);
    }
}