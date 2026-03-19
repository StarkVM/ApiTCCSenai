using System.Security.Cryptography;
using System.Text;
using UserAccess.Domain.Interfaces;



namespace UserAccess.Infrastructure.Security;

public sealed class VerificationCodeHasher  : IVerificationCodeHasher
{
    private readonly string _secretKey;

    public VerificationCodeHasher(string secretKey)
    {
        if (string.IsNullOrWhiteSpace(secretKey))
        {
            throw new InvalidOperationException("Code protection secret key was not configured.");
        }
        
        _secretKey = secretKey;
    }

    public string Hash(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Code cannot be empty.");
        }

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_secretKey));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(code));

        return Convert.ToHexString(hashBytes);
    }

    public bool Verify(string code, string codeHash)
    {
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(codeHash))
        {
            return false;
        }
        
        var computedHash = Hash(code);
        
        return CryptographicOperations.FixedTimeEquals(
            Convert.FromHexString(computedHash),
            Convert.FromHexString(codeHash));
    }
}