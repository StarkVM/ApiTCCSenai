using Listings.Contracts.Listings.Enums;
using Listings.Domain.Enums;

namespace Listings.Contracts.Listings.Records;

public record ListingForRentalSnapshot(
    Guid ListingId,
    Guid OwnerId,
    ListingContractStatus Status,
    bool IsFleet,
    decimal DailyPrice,
    bool OperatorAvailable,
    decimal OperatorDailyPrice,
    bool FreightAvailable,
    decimal FreightFixedPrice
    );