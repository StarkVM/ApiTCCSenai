using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using UserAccess.Application.Abstractions;

namespace UserAccess.Infrastructure.Storage;

/// <summary>
/// S3 implementation for temporary profile photo URLs.
/// / Implementação S3 para URLs temporárias de foto de perfil.
/// </summary>
public sealed class S3UserProfilePhotoUrlProvider : IUserProfilePhotoUrlProvider
{
    private readonly IAmazonS3 _amazonS3;
    private readonly UserProfilePhotoStorageOptions _options;

    public S3UserProfilePhotoUrlProvider(
        IAmazonS3 amazonS3,
        IOptions<UserProfilePhotoStorageOptions> options)
    {
        _amazonS3 = amazonS3;
        _options = options.Value;
    }

    public UserProfilePhotoAccessUrl Generate(
        string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
        {
            throw new ArgumentException("PROFILE_PHOTO_STORAGE_KEY_REQUIRED");
        }

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(
            _options.ReadUrlExpirationMinutes);

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = storageKey,
            Verb = HttpVerb.GET,
            Expires = expiresAtUtc
        };

        var url = _amazonS3.GetPreSignedURL(request);

        return new UserProfilePhotoAccessUrl(
            url,
            expiresAtUtc);
    }
}