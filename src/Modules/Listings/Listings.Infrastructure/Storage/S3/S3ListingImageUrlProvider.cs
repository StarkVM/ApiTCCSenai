using Amazon.S3;
using Amazon.S3.Model;
using Listings.Application.Abstractions;
using Listings.Domain.Interfaces;
using Microsoft.Extensions.Options;

namespace Listings.Infrastructure.Storage.S3;

/// <summary>
/// Generates temporary Amazon S3 URLs for listing images.
/// / Gera URLs temporárias do Amazon S3 para imagens de anúncios.
/// </summary>
public sealed class S3ListingImageUrlProvider : IListingImageUrlProvider
{
    private readonly IAmazonS3 _amazonS3;
    private readonly S3StorageOptions _options;
    private readonly IClock _clock;

    public S3ListingImageUrlProvider(
        IAmazonS3 amazonS3,
        IOptions<S3StorageOptions> options,
        IClock clock)
    {
        _amazonS3 = amazonS3;
        _options = options.Value;
        _clock = clock;
    }

    public ListingImageAccessUrl Generate(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
        {
            throw new ArgumentException(
                "Storage key cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(_options.BucketName))
        {
            throw new InvalidOperationException(
                "S3 bucket name was not configured.");
        }

        int expirationMinutes;

        if (_options.ReadUrlExpirationMinutes > 0 &&
            _options.ReadUrlExpirationMinutes <= 60)
        {
            expirationMinutes = _options.ReadUrlExpirationMinutes;
        }
        else
        {
            expirationMinutes = 15;
        }

        var expiresAtUtc = _clock.UtcNow
            .AddMinutes(expirationMinutes);

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = storageKey,
            Verb = HttpVerb.GET,
            Expires = expiresAtUtc
        };

        var url = _amazonS3.GetPreSignedURL(request);

        return new ListingImageAccessUrl(
            url,
            expiresAtUtc);
    }
}