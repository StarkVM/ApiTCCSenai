using Microsoft.Extensions.Options;
using UserAccess.Domain.Interfaces;

namespace UserAccess.Infrastructure.Auth.Options;

public class RefreshTokenLifetimeProvider : IRefreshTokenLifetimeProvider
{
    private readonly RefreshTokenOptions _refreshTokenOptions;
    
    public RefreshTokenLifetimeProvider(IOptions<RefreshTokenOptions> refreshTokenOptions)
    {
        _refreshTokenOptions = refreshTokenOptions.Value;
    }
    
    public DateTime GetExpirationDateUtc(DateTime nowUtc)
    {
        return nowUtc.AddDays(_refreshTokenOptions.RefreshTokenDays);
    }
}