using Amazon.S3;
using Amazon.S3.Model;
using Listings.Domain.Files;
using Listings.Domain.Interfaces;
using Microsoft.Extensions.Options;

namespace Listings.Infrastructure.Storage.S3;

/// <summary>
/// Amazon S3 implementation for listing image storage.
/// / Implementação Amazon S3 para armazenamento de imagens de anúncios.
/// </summary>
public sealed class S3ListingImageStorage : IListingImageStorage
{
    private static readonly HashSet<string> AllowedContentTypes = new(
        StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/webp"
    };
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
    
    /// <summary>
    /// Uploads a listing image to S3 and returns its storage key.
    /// / Envia uma imagem de anúncio para o S3 e retorna sua chave de armazenamento.
    /// </summary>
    public async Task<string> UpdateAsync(
        Guid listingId,
        ListingImageUpload image,
        int displayOrder,
        CancellationToken cancellationToken)
    {
        if (listingId == Guid.Empty)
        {
            throw new ArgumentException("LISTING_ID_REQUIRED");
        }

        if (image is null)
        {
            throw new ArgumentNullException(nameof(image));
        }

        if (string.IsNullOrWhiteSpace(image.FileName))
        {
            throw new ArgumentException("LISTING_IMAGE_FILE_NAME_REQUIRED");
        }

        if (string.IsNullOrWhiteSpace(image.ContentType))
        {
            throw new ArgumentException("LISTING_IMAGE_CONTENT_TYPE_REQUIRED");
        }

        if (!AllowedContentTypes.Contains(image.ContentType))
        {
            throw new ArgumentException("LISTING_IMAGE_CONTENT_TYPE_NOT_ALLOWED");
        }

        if (image.Length <= 0)
        {
            throw new ArgumentException("LISTING_IMAGE_EMPTY");
        }

        if (displayOrder <= 0)
        {
            throw new ArgumentException("LISTING_IMAGE_DISPLAY_ORDER_INVALID");
        }

        var extension = GetSafeExtension(
            image.FileName,
            image.ContentType);

        var normalizedBasePrefix = _options.BasePrefix
            .Trim()
            .Trim('/');

        var storageKey =
            $"{normalizedBasePrefix}/{listingId:N}/{Guid.NewGuid():N}{extension}";

        await using var stream = image.OpenReadStream();

        var request = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = storageKey,
            InputStream = stream,
            ContentType = image.ContentType
        };

        await _amazonS3.PutObjectAsync(
            request,
            cancellationToken);

        return storageKey;
    }

    /// <summary>
    /// Deletes a listing image from S3.
    /// / Remove uma imagem de anúncio do S3.
    /// </summary>
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