using System.Text.Json;
using UserAccess.Domain.Interfaces;
using UserAccess.Domain.Results;
using UserAccess.Infrastructure.IdentityVerification.Didit.Payloads.Records;

namespace UserAccess.Infrastructure.IdentityVerification.Didit.Payloads;

public class IdentityVerificationWebhookParser : IIdentityVerificationWebhookParser
{
    public IdentityVerificationWebhookParserResult Parse(string rawBody)
    {
        WebhookPayload? payload;

        payload = JsonSerializer.Deserialize<WebhookPayload>(
            rawBody,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
        );

        if (payload is null ||
            string.IsNullOrWhiteSpace(payload.SessionId) ||
            string.IsNullOrWhiteSpace(payload.Status))
        {
            throw new InvalidOperationException("Invalid payload");
        }

        return new IdentityVerificationWebhookParserResult(
            payload.SessionId,
            payload.Status,
            payload.WebhookType,
            payload.VendorData
        );

    }
}