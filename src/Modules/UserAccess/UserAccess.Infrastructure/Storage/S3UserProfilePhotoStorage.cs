using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using UserAccess.Domain.Files;
using UserAccess.Domain.Interfaces;

namespace UserAccess.Infrastructure.Storage;

/// <summary>
/// S3 implementation for user profile photo storage.
/// / Implementação S3 para armazenamento de foto de perfil do usuário.
/// </summary>
public sealed class S3UserProfilePhotoStorage : IUserProfilePhotoStorage
{
    private readonly IAmazonS3 _amazonS3;
    private readonly UserProfilePhotoStorageOptions _options;

    public S3UserProfilePhotoStorage(
        IAmazonS3 amazonS3,
        IOptions<UserProfilePhotoStorageOptions> options)
    {
        _amazonS3 = amazonS3;
        _options = options.Value;
    }

    public async Task<string> UploadAsync(
        Guid userId,
        UserProfilePhotoUpload photo,
        CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("USER_ID_REQUIRED");
        }

        var extension = GetSafeExtension(
            photo.FileName,
            photo.ContentType);

        var basePrefix = _options.BasePrefix
            .Trim()
            .Trim('/');

        var storageKey =
            $"{basePrefix}/{userId:N}/profile/{Guid.NewGuid():N}{extension}";

        await using var stream = photo.OpenReadStream();

        var request = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = storageKey,
            InputStream = stream,
            ContentType = photo.ContentType
        };

        await _amazonS3.PutObjectAsync(
            request,
            cancellationToken);

        return storageKey;
    }

    public async Task DeleteAsync(
        string storageKey,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
        {
            return;
        }

        var request = new DeleteObjectRequest
        {
            BucketName = _options.BucketName,
            Key = storageKey
        };

        await _amazonS3.DeleteObjectAsync(
            request,
            cancellationToken);
    }

    private static string GetSafeExtension(
        string fileName,
        string contentType)
    {
        var extension = Path.GetExtension(fileName)
            .ToLowerInvariant();

        if (extension is ".jpg" or ".jpeg" or ".png" or ".webp")
        {
            return extension;
        }

        return contentType.ToLowerInvariant() switch
        {
            "image/jpeg" => ".jpg",
            "image/jpg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => ".jpg"
        };
    }
}