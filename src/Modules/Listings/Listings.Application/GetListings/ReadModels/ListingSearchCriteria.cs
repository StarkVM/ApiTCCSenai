using Listings.Domain.Enums;

namespace Listings.Application.GetListings.ReadModels;

public sealed record ListingSearchCriteria(
    Guid? OwnerId,
    bool PublicOnly,
    string? Name,
    ListingCategory? Category,
    ListingStatus? Status,
    int Skip,
    int Take
);