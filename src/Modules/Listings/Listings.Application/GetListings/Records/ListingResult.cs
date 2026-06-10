using Listings.Domain.Enums;

namespace Listings.Application.GetListings.Records;

/// <summary>
/// Represents a listing returned to the client.
/// / Representa um anúncio retornado ao cliente.
/// </summary>
public sealed record ListingResult(
    Guid ListingId,
    Guid OwnerId,
    string? ProviderName,
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