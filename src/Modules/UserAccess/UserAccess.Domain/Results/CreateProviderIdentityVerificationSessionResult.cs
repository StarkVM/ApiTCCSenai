namespace UserAccess.Domain.Results;

public record CreateProviderIdentityVerificationSessionResult(
    string ProviderSessionId,
    string VerificationUrl,
    string SessionToken,
    string ProviderStatus
    );