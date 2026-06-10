using Rentals.Application.GetRentals.Enums;

namespace Rentals.Application.GetRentals.ReadModels;

/// <summary>
/// Represents rental search criteria.
/// / Representa os critérios de pesquisa de aluguéis.
/// </summary>
public sealed record RentalSearchCriteria(
    Guid UserId,
    RentalParticipantRole Role,
    RentalStatusFilter Status,
    int Skip,
    int Take
);