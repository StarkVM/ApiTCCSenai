using UserAccess.Domain.Entities;

namespace UserAccess.Domain.Interfaces;

public interface IEmailVerificationRepository
{
    Task AddAsync(EmailVerificationCode code);
}