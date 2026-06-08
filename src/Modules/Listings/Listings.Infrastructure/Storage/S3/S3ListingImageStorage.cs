using Amazon.S3;
using Amazon.S3.Model;
using Listings.Domain.Interfaces;
using Microsoft.Extensions.Options;

namespace Listings.Infrastructure.Storage.S3;

/// <summary>
/// Amazon S3 implementation for listing image storage.
/// / Implementação Amazon S3 para armazenamento de imagens de anúncios.
/// </summary>
public sealed class S3ListingImageStorage : IListingImageStorage
{
    private readonly IAmazonS3 _amazonS3;
    private readonly S3StorageOptions _options;

    public S3ListingImageStorage(
        IAmazonS3 amazonS3,
        IOptions<S3StorageOptions> options)
    {
        _amazonS3 = amazonS3;
        _options = options.Value;
    }

    public async Task<string> UploadAsync(
        Guid listingId,
        Guid imageId,
        string fileName,
        string contentType,
        Stream contentStream,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.BucketName))
        {
            throw new InvalidOperationException("S3 bucket name was not configured.");
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        var basePrefix = string.IsNullOrWhiteSpace(_options.BasePrefix)
            ? "listings"
            : _options.BasePrefix.Trim().Trim('/');

        var storageKey = $"{basePrefix}/{listingId}/images/{imageId}{extension}";

        var request = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = storageKey,
            InputStream = contentStream,
            ContentType = contentType,
            ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
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

        if (string.IsNullOrWhiteSpace(_options.BucketName))
        {
            throw new InvalidOperationException("S3 bucket name was not configured.");
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
}