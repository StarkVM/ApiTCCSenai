using Listings.Domain.Entities;
using Listings.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Listings.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core repository implementation for listings.
/// / Implementação de repositório EF Core para anúncios.
/// </summary>
public sealed class ListingRepository : IListingRepository
{
    private readonly ListingsDbContext _listingsDbContext;

    public ListingRepository(ListingsDbContext listingsDbContext)
    {
        _listingsDbContext = listingsDbContext;
    }

    /// <summary>
    /// Adds a new listing to the database context.
    /// / Adiciona um novo anúncio ao contexto do banco de dados.
    /// </summary>
    public async Task AddAsync(
        Listing listing,
        CancellationToken cancellationToken)
    {
        await _listingsDbContext.Listings.AddAsync(
            listing,
            cancellationToken);
    }

    /// <summary>
    /// Gets a listing by its identifier.
    /// / Busca um anúncio pelo seu identificador.
    /// </summary>
    public Task<Listing?> GetByIdAsync(
        Guid listingId,
        CancellationToken cancellationToken)
    {
        return _listingsDbContext.Listings
            .FirstOrDefaultAsync(
                listing => listing.Id == listingId,
                cancellationToken);
    }
}