namespace UserAccess.Domain.Interfaces;

public interface IRefreshTokenHasher
{
    string Hash(string refreshToken);
    
    bool Verify(string refreshToken, string refreshTokenHash);
}