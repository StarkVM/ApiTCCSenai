namespace UserAccess.Application.IdentityVerification.ProcessIdentityVerificationWebhook.Records;

public record ProcessIdentityVerificationWebhookCommand(
    string RawBody,
    string? SignatureV2,
    string? SignatureSimple,
    string? Timestamp
    );