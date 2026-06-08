namespace Listings.Application.CreateListings.Records;

public record CreateListingImageCommand(
    string FileName,
    string ContentType,
    long Length,
    Stream ContentStream
    );