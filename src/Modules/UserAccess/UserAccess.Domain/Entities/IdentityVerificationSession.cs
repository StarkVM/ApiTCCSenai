using UserAccess.Domain.Enums;

namespace UserAccess.Domain.Entities;

public class IdentityVerificationSession
{
    public Guid Id { get; private set; }
    
    public Guid UserId  { get; private set; }

    public User User { get; private set; } = default!;

    public string? ProviderSessionId { get; private set; } = default!;
    public string? ProviderSessionUrl { get; private set; } = default!;
    
    public IdentityVerificationStatus? Status { get; private set; }
    
    public IdentityVerificationProvider? Provider { get; private set; }
    
    public DateTime CreatedAtUtc { get; private set; }
    
    public DateTime? CompletedAtUtc { get; private set; }
    
    private IdentityVerificationSession(){}

    public IdentityVerificationSession(
        Guid id,
        Guid userId,
        IdentityVerificationProvider provider,
        DateTime createdAtUtc)
    {
        Id = id;
        UserId = userId;
        CreatedAtUtc = createdAtUtc;
        Provider = provider;
        Status = IdentityVerificationStatus.Pending;
    }

    public void AttachProviderSession(string sessionId, string sessionUrl)
    {
        ProviderSessionId = sessionId;
        ProviderSessionUrl = sessionUrl;
    }
    
    public void MarkApproved(DateTime completedAtUtc)
    {
        Status = IdentityVerificationStatus.Approved;
        CompletedAtUtc = completedAtUtc;
    }

    public void MarkDenied(DateTime completedAtUtc)
    {
        Status = IdentityVerificationStatus.Denied;
        CompletedAtUtc = completedAtUtc;
    }
    
    public void MarkFailed(DateTime completedAtUtc)
    {
        Status = IdentityVerificationStatus.Failed;
        CompletedAtUtc = completedAtUtc;
    }
    public void MarkExpired(DateTime completedAtUtc)
    {
        Status = IdentityVerificationStatus.Expired;
        CompletedAtUtc = completedAtUtc;
    }
}