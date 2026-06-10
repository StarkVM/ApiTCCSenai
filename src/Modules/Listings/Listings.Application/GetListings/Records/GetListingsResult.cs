namespace Listings.Application.GetListings.Records;

/// <summary>
/// Represents a paginated listing search result.
/// / Representa um resultado paginado de pesquisa de anúncios.
/// </summary>
public sealed record GetListingsResult(
    IReadOnlyCollection<ListingResult> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages
);