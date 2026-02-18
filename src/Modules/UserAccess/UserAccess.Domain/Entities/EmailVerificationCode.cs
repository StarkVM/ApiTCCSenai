namespace UserAccess.Domain.Entities;

public class EmailVerificationCode
{
    public Guid Id { get; private set; }
    
    public Guid UserId { get; private set; }
    public User User { get; private set; } = default!;

    public string CodeHash { get; private set; } = default!;
    public DateTime ExpiresAt { get; private set; }
    
    public DateTime? ConsumedAt { get; private set; }
    public int Attempts { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    
    private EmailVerificationCode() {}

    public EmailVerificationCode(Guid id, Guid userId, string codeHash, DateTime createdAtUtc, DateTime expiresAtUtc)
    {
        Id = id;
        UserId = userId;
        CodeHash = codeHash;
        
        CreatedAt = createdAtUtc;
        ExpiresAt = expiresAtUtc;
        
        Attempts = 0;
    }

    public bool IsExpired(DateTime nowUtc) => nowUtc >= ExpiresAt;
    
    public bool IsConsumed() => ConsumedAt.HasValue;
    
    public void IncrementAttempts() => Attempts++;
        
    public void Consume (DateTime nowUtc) => ConsumedAt = nowUtc;
}