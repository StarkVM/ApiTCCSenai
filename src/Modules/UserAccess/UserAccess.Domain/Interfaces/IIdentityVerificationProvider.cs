using UserAccess.Domain.Results;
using UserAccess.Domain.Senders;

namespace UserAccess.Domain.Interfaces;

public interface IIdentityVerificationProvider
{
    Task<CreateProviderIdentityVerificationSessionResult> CreateSessionAsync(
        CreateProviderIdentityVerificationSessionRequest request,
        CancellationToken cancellationToken);
}