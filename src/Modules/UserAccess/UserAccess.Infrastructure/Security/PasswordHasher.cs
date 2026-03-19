using UserAccess.Domain.Interfaces;

namespace UserAccess.Infrastructure.Security;

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            throw new ArgumentException("Password cannot be null or empty");
        }

        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11);
    }

    public bool Verify(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }
        
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}