using Microsoft.Extensions.Options;
using UserAccess.Domain.Interfaces;

namespace UserAccess.Infrastructure.Auth.Options;
 
public class AccessTokenLifetimeProvider : IAccessTokenLifetimeProvider
{
    private readonly JwtOptions _jwtOptions;
    
    public AccessTokenLifetimeProvider(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }
    
    public DateTime GetExpirationDateUtc(DateTime nowUtc)
    {
        return nowUtc.AddMinutes(_jwtOptions.AccessTokenMinutes);
    }
}