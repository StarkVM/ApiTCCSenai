using UserAccess.Domain.Entities;
using UserAccess.Domain.Enums;

namespace UserAccess.Domain.Interfaces;

public interface IVerificationCodeRepository
{
    Task AddAsync(EmailVerificationCode code, CancellationToken cancellationToken);
    
    Task<EmailVerificationCode?> GetLatestActiveAsync(Guid userId, VerificationCodePurpose purpose ,CancellationToken cancellationToken);
}