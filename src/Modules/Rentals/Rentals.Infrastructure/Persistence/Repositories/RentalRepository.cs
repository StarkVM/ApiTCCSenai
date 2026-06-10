using Microsoft.EntityFrameworkCore;
using Rentals.Domain.Entities;
using Rentals.Domain.Enums;
using Rentals.Domain.Interfaces;

namespace Rentals.Infrastructure.Persistence.Repositories;

public sealed class RentalRepository : IRentalRepository
{
    private readonly RentalsDbContext _rentalsDbContext;

    public RentalRepository(RentalsDbContext rentalsDbContext)
    {
        _rentalsDbContext = rentalsDbContext;
    }

    public async Task AddAsync(
        Rental rental,
        CancellationToken cancellationToken)
    {
        await _rentalsDbContext.Rentals.AddAsync(
            rental,
            cancellationToken);
    }

    public Task<bool> ExistsActiveRentalForListingAsync(
        Guid listingId,
        CancellationToken cancellationToken)
    {
        return _rentalsDbContext.Rentals
            .AsNoTracking()
            .AnyAsync(
                rental =>
                    rental.ListingId == listingId &&
                    (
                        rental.Status == RentalStatus.Approved ||
                        rental.Status == RentalStatus.InProgress
                    ),
                cancellationToken);
    }
    
    public Task<Rental?> GetByIdAsync(
        Guid rentalId,
        CancellationToken cancellationToken)
    {
        return _rentalsDbContext.Rentals
            .SingleOrDefaultAsync(
                rental => rental.Id == rentalId,
                cancellationToken);
    }
}