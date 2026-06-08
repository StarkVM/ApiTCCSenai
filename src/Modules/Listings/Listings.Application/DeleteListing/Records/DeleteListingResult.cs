using Listings.Domain.Enums;

namespace Listings.Application.DeleteListing.Records;

public record DeleteListingResult( 
    Guid ListingId,
    ListingStatus Status,
    DateTime UpdatedAtUtc);