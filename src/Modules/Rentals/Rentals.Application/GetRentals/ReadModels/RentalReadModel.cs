using Rentals.Domain.Enums;

namespace Rentals.Application.GetRentals.ReadModels;

/// <summary>
/// Represents rental data returned by the persistence query.
/// / Representa os dados do aluguel retornados pela consulta de persistência.
/// </summary>
public sealed record RentalReadModel(
    Guid RentalId,
    Guid ListingId,
    Guid ProviderId,
    Guid RenterId,
    RentalStatus Status,
    DateOnly StartDate,
    DateOnly EndDate,
    int TotalDays,
    bool IncludeOperator,
    bool IncludeFreight,
    decimal ListingDailyPriceSnapshot,
    decimal OperatorDailyPriceSnapshot,
    decimal FreightFixedPriceSnapshot,
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