using Rentals.Domain.Enums;

namespace Rentals.Application.GetRentals.Records;

/// <summary>
/// Represents a rental returned to the client.
/// / Representa um aluguel retornado ao cliente.
/// </summary>
public sealed record RentalResult(
    Guid RentalId,
    Guid ListingId,
    Guid ProviderId,
    string? ProviderName,
    Guid RenterId,
    string? RenterName,
    RentalStatus Status,
    DateOnly StartDate,
    DateOnly EndDate,
    int TotalDays,
    bool IncludeOperator,
    bool IncludeFreight,
    decimal ListingDailyPrice,
    decimal OperatorDailyPrice,
    decimal FreightFixedPrice,
    decimal MachineSubtotal,
    decimal OperatorSubtotal,
    decimal FreightSubtotal,
    decimal TotalAmount,
    DateTime CreatedAtUtc,
    DateTime ApprovedAtUtc,
    DateTime UpdatedAtUtc,
    DateTime? StartedAtUtc,
    DateTime? CompletedAtUtc,
    DateTime? CancelledAtUtc,
    Guid? CompletedByUserId
);