using Listings.Domain.Enums;

namespace Api.Routes.Listings.Requests;

/// <summary>
/// Query string parameters used to search listings.
/// / Parâmetros da query string usados para pesquisar anúncios.
/// </summary>
public sealed class GetListingsRequest
{
    /// <summary>
    /// Indicates whether only the authenticated user's listings should be returned.
    /// / Indica se apenas os anúncios do usuário autenticado devem ser retornados.
    /// </summary>
    public bool? Mine { get; init; }

    /// <summary>
    /// Optional listing title filter.
    /// / Filtro opcional pelo título do anúncio.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Optional listing category filter.
    /// / Filtro opcional pela categoria do anúncio.
    /// </summary>
    public ListingCategory? Category { get; init; }

    /// <summary>
    /// Optional status filter available only for the user's own listings.
    /// / Filtro opcional por status disponível apenas para os próprios anúncios.
    /// </summary>
    public ListingStatus? Status { get; init; }

    /// <summary>
    /// Requested page number.
    /// / Número da página solicitada.
    /// </summary>
    public int? Page { get; init; } = 1;

    /// <summary>
    /// Number of items returned per page.
    /// / Quantidade de itens retornados por página.
    /// </summary>
    public int? PageSize { get; init; } = 20;
}