namespace Listings.Domain.Files;

public sealed record ListingImageUpload(
    string FileName,
    string ContentType,
    long Length,
    Func<Stream> OpenReadStream
);