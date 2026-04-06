using System.Text;
using System.Security.Cryptography;
using UserAccess.Domain.Interfaces;

namespace UserAccess.Infrastructure.Auth;

public sealed class RefreshTokenHasher : IRefreshTokenHasher
{   
    public string Hash(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            throw new ArgumentNullException("RefreshToken cannot be null or empty.");
        }
        
        var refreshTokenBytes = Encoding.UTF8.GetBytes(refreshToken);
        var hashBytes = SHA256.HashData(refreshTokenBytes);
        return Convert.ToHexString(hashBytes);
    }

    public bool Verify(string refreshToken, string refreshTokenHash)
    {
        if (string.IsNullOrWhiteSpace(refreshToken) || string.IsNullOrWhiteSpace(refreshTokenHash))
        {
            return false;
        }
        var computedHash = Hash(refreshToken);
        return CryptographicOperations.FixedTimeEquals
        (
            Convert.FromHexString(computedHash),
            Convert.FromHexString(refreshTokenHash));
    }
}