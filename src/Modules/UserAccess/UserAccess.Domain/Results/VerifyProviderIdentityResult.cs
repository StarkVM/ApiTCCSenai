namespace UserAccess.Domain.Results;

public record VerifyProviderIdentityResult(
    bool IsValid,
    string? ProviderStatus,
    string? FailureReason);