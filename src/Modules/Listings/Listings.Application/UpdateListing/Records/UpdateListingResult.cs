using Listings.Domain.Enums;

namespace Listings.Application.UpdateListing.Records;

/// <summary>
/// Result returned after updating a listing.
/// / Resultado retornado após atualizar um anúncio.
/// </summary>
public sealed record UpdateListingResult(
    Guid ListingId,
    ListingStatus Status,
    DateTime UpdatedAtUtc
);