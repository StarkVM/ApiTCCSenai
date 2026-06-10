using Rentals.Domain.Enums;

namespace Rentals.Application.CancelRental.Records;

public sealed record CancelRentalResult(
    Guid RentalId,
    Guid ListingId,
    Guid ProviderId,
    Guid RenterId,
    RentalStatus Status,
    Guid CancelledByUserId,
    decimal CancellationPenaltyAmount,
    DateTime CancelledAtUtc
);