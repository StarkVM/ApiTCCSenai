using Listings.Domain.Interfaces;

namespace Listings.Infrastructure.Persistence.Repositories;

/// <summary>
/// Unit of work implementation for the Listings module.
/// / Implementação de unidade de trabalho do módulo de anúncios.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ListingsDbContext _listingsDbContext;

    public UnitOfWork(ListingsDbContext listingsDbContext)
    {
        _listingsDbContext = listingsDbContext;
    }
    
    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _listingsDbContext.SaveChangesAsync(cancellationToken);
    }
}