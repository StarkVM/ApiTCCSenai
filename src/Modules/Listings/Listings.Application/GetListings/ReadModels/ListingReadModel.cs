using Listings.Application.GetListings.Records;
using Listings.Domain.Enums;

namespace Listings.Application.GetListings.ReadModels;

public sealed record ListingReadModel(
    Guid ListingId,
    Guid OwnerId,
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
    IReadOnlyCollection<ListingImageReadModel> Images
    );