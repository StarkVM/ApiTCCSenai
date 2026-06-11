using Listings.Application.GetListings.Records;
using Listings.Domain.Enums;

namespace Listings.Application.GetListingById.Records;

/// <summary>
/// Result returned when getting a public listing by id.
/// / Resultado retornado ao buscar um anúncio público por id.
/// </summary>
public sealed record GetListingByIdResult(
    Guid ListingId,
    Guid OwnerId,
    string? ProviderName,
    string? ProviderProfilePhotoUrl,
    DateTime? ProviderProfilePhotoUrlExpiresAtUtc,
    string Title,
    string Description,
    ListingCategory Category,
    decimal DailyPrice,
    bool IsFleet,
    ListingStatus Status,
    bool OperatorAvailable,
    decimal OperatorDailyPrice,
    bool FreightAvailable,
    decimal FreightFixedPrice,
    string PickupState,
    string PickupCity,
    string PickupDistrict,
    string PickupStreet,
    string PickupNumber,
    string PickupZipCode,
    string? PickupComplement,
    string? RejectionReason,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    IReadOnlyCollection<ListingImageResult> Images
);