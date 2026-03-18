using UserAccess.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using UserAccess.Domain.Entities;

namespace UserAccess.Infrastructure.Persistence.Repositories;

public class AddressRepository : IAddressRepository
{
    private readonly UserAccessDbContext _userAccessDbContext;

    public AddressRepository(UserAccessDbContext userAccessDbContext)
    {
        _userAccessDbContext = userAccessDbContext;
    }
    
    public async Task AddAddressAsync(Address address, CancellationToken cancellationToken)
    {
        await _userAccessDbContext.Addresses.AddAsync(address, cancellationToken);
    }

    public Task<Address?> GetAddressByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return _userAccessDbContext.Addresses.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }
}