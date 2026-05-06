using UserAccess.Domain.Entities;

namespace UserAccess.Domain.Interfaces;

public interface IIdentityVerificationRepository
{
    Task AddAsync(IdentityVerificationSession session, CancellationToken cancellationToken);

    Task<IdentityVerificationSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IdentityVerificationSession?> GetByProviderSessionIdAsync(string provider, string providerSessionId, CancellationToken cancellationToken);

    Task<IdentityVerificationSession?> GetLatestByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}