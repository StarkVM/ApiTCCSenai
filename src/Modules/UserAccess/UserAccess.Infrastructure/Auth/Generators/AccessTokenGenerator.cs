using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UserAccess.Domain.Entities;
using UserAccess.Domain.Interfaces;
using UserAccess.Infrastructure.Auth.Options;

namespace UserAccess.Infrastructure.Auth.Generators;

public class AccessTokenGenerator : IAccessTokenGenerator
{
    private readonly JwtOptions _jwtOptions;
    private readonly IClock _clock;
    public AccessTokenGenerator(IOptions<JwtOptions> jwtOptions,  IClock clock)
    {
        _jwtOptions = jwtOptions.Value;
        _clock = clock;
    }
    
    public string Generate(User user)
    {
        if (user is null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        if (string.IsNullOrWhiteSpace(_jwtOptions.SigningKey))
        {
            throw new InvalidOperationException("Jwt Signingkey is not Configured");
        }

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));

        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256
        );
        
        var nowUtc = _clock.UtcNow;

        var expiresAtUtc = nowUtc.AddMinutes(_jwtOptions.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email)
        };

        var jwtToken = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            notBefore: nowUtc,
            expires: expiresAtUtc,
            signingCredentials: credentials
        );
        
        return new JwtSecurityTokenHandler().WriteToken(jwtToken);
    }
}