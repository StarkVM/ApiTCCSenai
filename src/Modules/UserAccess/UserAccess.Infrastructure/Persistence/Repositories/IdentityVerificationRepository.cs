using Microsoft.EntityFrameworkCore;
using UserAccess.Domain.Entities;
using UserAccess.Domain.Enums;
using UserAccess.Domain.Interfaces;

namespace UserAccess.Infrastructure.Persistence.Repositories;

public class IdentityVerificationRepository : IIdentityVerificationRepository
{
    UserAccessDbContext _userAccessDbContext;

    public IdentityVerificationRepository(UserAccessDbContext userAccessDbContext)
    {
        _userAccessDbContext = userAccessDbContext;
    }
    public async Task AddAsync(IdentityVerificationSession session, CancellationToken cancellationToken)
    {
        await _userAccessDbContext.AddAsync(session, cancellationToken);
    }

    public async Task<IdentityVerificationSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _userAccessDbContext.IdentityVerificationSessions.FirstOrDefaultAsync
        (x => x.Id == id, cancellationToken);
    }

    public async Task<IdentityVerificationSession?> GetByProviderSessionIdAsync(string provider, string providerSessionId, CancellationToken cancellationToken)
    {
        return await _userAccessDbContext.IdentityVerificationSessions.FirstOrDefaultAsync
        (x => x.ProviderSessionId == providerSessionId, cancellationToken);
    }

    public async Task<IdentityVerificationSession?> GetLatestByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _userAccessDbContext.IdentityVerificationSessions
            //.Include(x => x.User)
            .Where(x => 
                x.UserId == userId &&
                x.Status == IdentityVerificationStatus.Pending)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }
}