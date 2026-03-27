namespace UserAccess.Domain.Entities;

public sealed class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public User User { get; private set; } = default!;
    
    public string TokenHash { get; private set; } = default!;
    
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }
    
    public string? RevokedReason { get; private set; }
    public string? ReplacedByTokenHash { get; private set; }
    
    public bool IsExpired(DateTime utcNow) => ExpiresAtUtc <= utcNow;
    public bool IsRevoked() => RevokedAtUtc.HasValue;
    public bool IsActive(DateTime utcNow) => !IsExpired(utcNow) && !IsRevoked();
    
    private RefreshToken(){}

    public RefreshToken(
        Guid id,
        Guid userId,
        string tokenHash,
        DateTime createdAtUtc,
        DateTime expiresAtUtc)
    {
        if (createdAtUtc >= expiresAtUtc)
        {
            throw new ArgumentException("Expiration date must be after creation date");
        }
        
        Id = id;
        UserId = userId;
        TokenHash = tokenHash;
        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
    }

    public void Revoke(DateTime revokeAtUtcNow, string? replacedByTokenHash, string? reason)
    {
        if (IsRevoked())
        {
            throw new InvalidOperationException("Refresh token already revoked");
        }
        
        RevokedAtUtc = revokeAtUtcNow;
        RevokedReason = reason;
        ReplacedByTokenHash = replacedByTokenHash;
    }
}