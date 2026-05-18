using System.Security.Cryptography;
using System.Text;
using UserAccess.Domain.Helpers;
using UserAccess.Domain.Interfaces;

namespace UserAccess.Infrastructure.Security;

public class CpfHasher : ICpfHasher
{
    private readonly string _secretKey;

    public CpfHasher(string secretKey)
    {
        if (string.IsNullOrWhiteSpace(secretKey))
        {
            throw new InvalidOperationException("CPF protection secret key was not configured.");
        }
        
        _secretKey = secretKey;
    }

    public string Hash(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
        {
            throw new ArgumentException("Cpf cannot be empty.");
        }

        var normalizeCpf = new string(cpf.Where(char.IsDigit).ToArray());

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_secretKey));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(normalizeCpf));

        return Convert.ToHexString(hashBytes);
    }

    public bool Verify(string cpf, string hash)
    {
        cpf.Clean();
        var normalizeCpf = new string(cpf.Where(char.IsDigit).ToArray());
        var newHash = Hash(normalizeCpf);

        if (string.Equals(
                hash,
                newHash,
                StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        
        return false;
    }
}