using UserAccess.Domain.Entities;

namespace UserAccess.Domain.Interfaces;

public interface IAddressRepository
{
    Task AddAddressAsync(Address address, CancellationToken cancellationToken);
    
    Task<Address?> GetAddressByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}