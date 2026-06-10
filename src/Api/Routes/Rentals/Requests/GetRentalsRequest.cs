using Rentals.Application.GetRentals.Enums;

namespace Api.Routes.Rentals.Requests;

/// <summary>
/// Query string parameters used to search the authenticated user's rentals.
/// / Parâmetros da query string usados para pesquisar os aluguéis do usuário autenticado.
/// </summary>
public sealed class GetRentalsRequest
{
    /// <summary>
    /// Role of the authenticated user in the rentals.
    /// / Papel do usuário autenticado nos aluguéis.
    /// </summary>
    public RentalParticipantRole? Role { get; init; }

    /// <summary>
    /// Optional rental status group.
    /// / Grupo opcional de status do aluguel.
    /// </summary>
    public RentalStatusFilter? Status { get; init; }

    /// <summary>
    /// Requested page number.
    /// / Número da página solicitada.
    /// </summary>
    public int? Page { get; init; }

    /// <summary>
    /// Number of items returned per page.
    /// / Quantidade de itens retornados por página.
    /// </summary>
    public int? PageSize { get; init; }
}