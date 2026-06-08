namespace Listings.Application.DeleteListing.Records;

public record DeleteListingCommand(
    Guid ListingId,
    Guid RequesterId);