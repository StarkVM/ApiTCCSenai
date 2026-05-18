namespace UserAccess.Domain.Results;

public record IdentityVerificationWebhookParserResult(
    string ProviderSessionId,
    string ProviderStatus,
    string? ProviderEventType,
    string? VendorData
    );