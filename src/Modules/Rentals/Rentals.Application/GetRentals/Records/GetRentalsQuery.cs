using Rentals.Application.GetRentals.Enums;

namespace Rentals.Application.GetRentals.Records;

/// <summary>
/// Query used to search the authenticated user's rentals.
/// / Consulta usada para pesquisar os aluguéis do usuário autenticado.
/// </summary>
public sealed record GetRentalsQuery(
    Guid UserId,
    RentalParticipantRole? Role,
    RentalStatusFilter Status,
    int Page,
    int PageSize
);