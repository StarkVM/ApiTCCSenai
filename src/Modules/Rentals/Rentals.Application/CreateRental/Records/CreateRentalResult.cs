using Rentals.Domain.Enums;

namespace Rentals.Application.CreateRental.Records;

public record CreateRentalResult(
    Guid RentalId,
    Guid ListingId,
    Guid OwnerId,
    Guid RenterId,
    RentalStatus Status,
    DateOnly StartDate,
    DateOnly EndDate,
    int TotalDays,
    bool IncludeOperator,
    bool IncludeFreight,
    decimal MachineSubtotal,
    decimal OperatorSubtotal,
    decimal FreightSubtotal,
    decimal TotalAmount,
    DateTime CreatedAtUtc
    );