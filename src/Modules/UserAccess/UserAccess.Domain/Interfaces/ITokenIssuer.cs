using UserAccess.Domain.Entities;
using UserAccess.Domain.Results;

namespace UserAccess.Domain.Interfaces;

public interface ITokenIssuer
{
    public Task<AuthTokensResult> IssueAsync(User? user, CancellationToken cancellationToken);
}