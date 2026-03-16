using UserAccess.Domain.Interfaces;

namespace UserAccess.Infrastructure.Security;

public sealed class VerificationCodeHasher  : IVerificationCodeHasher
{
    public string Hash(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            throw new ArgumentException("Code cannot be null or empty");
        }

        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11);
    }

    public bool Verify(string code, string codeHash)
    {
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(codeHash))
        {
            return false;
        }
        
        return BCrypt.Net.BCrypt.Verify(codeHash, code);
    }
}