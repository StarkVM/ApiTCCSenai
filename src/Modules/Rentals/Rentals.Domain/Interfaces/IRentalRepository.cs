using Rentals.Domain.Entities;

namespace Rentals.Domain.Interfaces;

/// <summary>
/// Defines persistence operations for rentals.
/// / Define as operações de persistência para aluguéis.
/// </summary>
public interface IRentalRepository
{
    Task AddAsync(
        Rental rental,
        CancellationToken cancellationToken);
    
    Task<bool> ExistsActiveRentalForListingAsync(
        Guid listingId,
        CancellationToken cancellationToken);
    
    Task<Rental?> GetByIdAsync(
        Guid rentalId,
        CancellationToken cancellationToken);
}