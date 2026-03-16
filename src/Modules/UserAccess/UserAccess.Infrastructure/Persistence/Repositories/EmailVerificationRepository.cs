using UserAccess.Domain.Entities;
using UserAccess.Domain.Interfaces;

namespace UserAccess.Infrastructure.Persistence.Repositories;

public class EmailVerificationRepository  : IEmailVerificationRepository
{
    private readonly UserAccessDbContext _userAccessDbContext;

    public EmailVerificationRepository(UserAccessDbContext userAccessDbContext)
    {
        _userAccessDbContext = userAccessDbContext;
    }
    
    public async Task AddAsync(EmailVerificationCode code, CancellationToken cancellationToken)
    {
        await _userAccessDbContext.EmailVerificationCodes.AddAsync(code, cancellationToken);
    }
}