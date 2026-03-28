using Microsoft.EntityFrameworkCore;
using UserAccess.Domain.Entities;
using UserAccess.Domain.Interfaces;

namespace UserAccess.Infrastructure.Persistence.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly UserAccessDbContext _userAccessDbContext;
    private readonly IClock _clock;

    public RefreshTokenRepository(UserAccessDbContext userAccessDbContext, IClock clock)
    {
        _userAccessDbContext = userAccessDbContext;
        _clock = clock;
    }
    
    public async Task AddAsync(RefreshToken token, CancellationToken cancellationToken)
    {
        await _userAccessDbContext.RefreshTokens.AddAsync(token, cancellationToken);
    }

    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken)
    {
        return await _userAccessDbContext.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash,  cancellationToken);
    }

    public async Task<List<RefreshToken>> GetActiveByUseridAsync(Guid userId, CancellationToken cancellationToken)
    {
        var nowUtc = _clock.UtcNow;
        
        return await _userAccessDbContext.RefreshTokens
            .Where(x => 
            x.UserId == userId &&
            x.RevokedAtUtc == null &&
            x.ExpiresAtUtc > nowUtc
            ).ToListAsync(cancellationToken);
        
    }
}