using Microsoft.EntityFrameworkCore;
using UserAccess.Domain.Entities;
using UserAccess.Domain.Enums;
using UserAccess.Domain.Interfaces;

namespace UserAccess.Infrastructure.Persistence.Repositories;

public class VerificationCodeRepository  : IVerificationCodeRepository
{
    private readonly UserAccessDbContext _userAccessDbContext;
    private readonly IClock _clock;

    public VerificationCodeRepository(UserAccessDbContext userAccessDbContext, IClock clock)
    {
        _userAccessDbContext = userAccessDbContext;
        _clock = clock;
    }
    
    public async Task AddAsync(EmailVerificationCode code, CancellationToken cancellationToken)
    {
        await _userAccessDbContext.EmailVerificationCodes.AddAsync(code, cancellationToken);
    }

    public async Task<EmailVerificationCode?> GetLatestActiveAsync(Guid userId, VerificationCodePurpose purpose,
        CancellationToken cancellationToken)
    {
        return await _userAccessDbContext.EmailVerificationCodes.Where
        (x => x.UserId == userId &&
              x.Purpose == purpose &&
              x.ConsumedAt == null &&
              x.ExpiresAt > _clock.UtcNow)
            .OrderByDescending(x => x.ExpiresAt)
            .FirstOrDefaultAsync(cancellationToken);
    }
}