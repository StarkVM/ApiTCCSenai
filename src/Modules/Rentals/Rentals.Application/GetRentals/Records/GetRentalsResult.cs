namespace Rentals.Application.GetRentals.Records;

/// <summary>
/// Represents a paginated rental search result.
/// / Representa um resultado paginado de pesquisa de aluguéis.
/// </summary>
public sealed record GetRentalsResult(
    IReadOnlyCollection<RentalResult> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages
);