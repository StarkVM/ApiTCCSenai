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

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken)
    {
        return _userAccessDbContext.Users.AnyAsync(u => u.Email == email, cancellationToken);
    }
    
    public Task<bool> CpfHashExistsAsync(string cpfHash, CancellationToken cancellationToken)
    {
        return _userAccessDbContext.Users.AnyAsync(u => u.CpfHash == cpfHash, cancellationToken );
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        await _userAccessDbContext.Users.AddAsync(user, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _userAccessDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DisableExpiredAsync(DateTime utcNow, CancellationToken cancellationToken)
    {
        var users = await _userAccessDbContext.EmailVerificationCodes.
            Where(x => x.ExpiresAt < utcNow).
            Select(x => x.User).ToListAsync(cancellationToken);

        foreach (var user in users)
        {
            if (user.Status == UserStatus.PendingEmailVerification)
            {
                user.Disable();
            }
        }
        
    }
    
    public async Task DeleteDisabledByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var users = await _userAccessDbContext.Users.
            Where(x => x.Email == email && x.Status == UserStatus.Disabled).ToListAsync(cancellationToken);
        
        _userAccessDbContext.Users.RemoveRange(users);
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