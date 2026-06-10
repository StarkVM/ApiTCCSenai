using Rentals.Domain.Interfaces;

namespace Rentals.Infrastructure.Persistence.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly RentalsDbContext _rentalsDbContext;

    public UnitOfWork(RentalsDbContext rentalsDbContext)
    {
        _rentalsDbContext = rentalsDbContext;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _rentalsDbContext.SaveChangesAsync(cancellationToken);
    }
}