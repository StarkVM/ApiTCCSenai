namespace UserAccess.Domain.Interfaces;

public interface IIdentityVerificationWebhookAuthenticator
{
    Task<bool> IsAuthentic(
        string rawBody,
        string? signatureV2,
        string? signatureSimple,
        string? timestamp
        );
}