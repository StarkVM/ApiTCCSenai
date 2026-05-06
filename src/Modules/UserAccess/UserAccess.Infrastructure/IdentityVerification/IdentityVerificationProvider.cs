using UserAccess.Domain.Interfaces;
using UserAccess.Domain.Results;
using UserAccess.Domain.Senders;

namespace UserAccess.Infrastructure.IdentityVerification;

public class IdentityVerificationProvider : IIdentityVerificationProvider
{
    public Task<CreateProviderIdentityVerificationSessionResult> CreateSessionAsync(
        CreateProviderIdentityVerificationSessionRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}