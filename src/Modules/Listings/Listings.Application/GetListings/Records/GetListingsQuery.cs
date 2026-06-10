using Listings.Domain.Enums;

namespace Listings.Application.GetListings.Records;

public record GetListingsQuery(
    Guid? RequesterId,
    bool Mine,
    string? Name,
    ListingCategory? Category,
    ListingStatus? Status,
    int Page,
    int PageSize
    );