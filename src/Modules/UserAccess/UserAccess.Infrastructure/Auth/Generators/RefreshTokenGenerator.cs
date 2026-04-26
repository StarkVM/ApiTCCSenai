using System.Security.Cryptography;
using UserAccess.Domain.Interfaces;

namespace UserAccess.Infrastructure.Auth.Generators;

public sealed class RefreshTokenGenerator : IRefreshTokenGenerator
{
    public string Generate()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(32);
        
        return Convert.ToBase64String(randomBytes);
    }
}