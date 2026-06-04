using Listings.Domain.Entities;

namespace Listings.Domain.Interfaces;

/// <summary>
/// Defines persistence operations for listings.
/// / Define as operações de persistência para anúncios.
/// </summary>
public interface IListingRepository
{
    /// <summary>
    /// Adds a new listing to the persistence context.
    /// / Adiciona um novo anúncio ao contexto de persistência.
    /// </summary>
    Task AddAsync(
        Listing listing,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets a listing by its identifier.
    /// / Busca um anúncio pelo seu identificador.
    /// </summary>
    Task<Listing?> GetByIdAsync(
        Guid listingId,
        CancellationToken cancellationToken);
}