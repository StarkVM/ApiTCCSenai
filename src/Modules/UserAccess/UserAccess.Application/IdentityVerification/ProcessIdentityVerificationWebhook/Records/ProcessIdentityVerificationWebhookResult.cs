namespace UserAccess.Application.IdentityVerification.ProcessIdentityVerificationWebhook.Records;

public record ProcessIdentityVerificationWebhookResult(
    bool Success,
    string Code
    );