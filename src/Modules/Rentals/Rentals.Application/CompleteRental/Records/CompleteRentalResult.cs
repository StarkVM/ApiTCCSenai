using Rentals.Domain.Enums;

namespace Rentals.Application.CompleteRental.Records;

public sealed record CompleteRentalResult(
    Guid RentalId,
    Guid ListingId,
    Guid ProviderId,
    Guid RenterId,
    RentalStatus Status,
    Guid CompletedByUserId,
    DateTime CompletedAtUtc
);