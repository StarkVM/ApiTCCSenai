using UserAccess.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using UserAccess.Domain.Entities;
using UserAccess.Domain.Enums;

namespace UserAccess.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly UserAccessDbContext _userAccessDbContext;
    
    public UserRepository(UserAccessDbContext dbContext)
    {
        _userAccessDbContext = dbContext;
    }

    public async Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        
        return await _userAccessDbContext.Users
            .Include(x => x.Address)
            .FirstOrDefaultAsync(x => x.Id == userId && x.Status != UserStatus.PendingEmailVerification, cancellationToken);
    }
    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken)
    {
        return await _userAccessDbContext.Users.AnyAsync(u => u.Email == email, cancellationToken);
    }
    
    public async Task<bool> CpfHashExistsAsync(string cpfHash, CancellationToken cancellationToken)
    {
        return await _userAccessDbContext.Users.AnyAsync(u => u.CpfHash == cpfHash, cancellationToken );
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        await _userAccessDbContext.Users.AddAsync(user, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _userAccessDbContext.SaveChangesAsync(cancellationToken);
    }
    
    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
         return _userAccessDbContext.Users.
            FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }
    
    public Task<User?> GetByCpfAsync(string cpfHash, CancellationToken cancellationToken)
    {
        return _userAccessDbContext.Users.
            FirstOrDefaultAsync(x => x.CpfHash == cpfHash, cancellationToken);
    }

    public Task<bool> CpfHashExistsForAnotherUserAsync(string cpfHash, Guid userId, CancellationToken cancellationToken)
    {
        return _userAccessDbContext.Users.AnyAsync(x => x.CpfHash == cpfHash && x.Id != userId, cancellationToken);
    }
}