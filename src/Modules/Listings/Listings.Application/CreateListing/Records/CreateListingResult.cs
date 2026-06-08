using Listings.Domain.Enums;

namespace Listings.Application.CreateListings.Records;

public record CreateListingResult(
    Guid ListingId,
    ListingStatus Status,
    DateTime CreatedAtUtc);