using System.Text.Json.Serialization;

namespace UserAccess.Infrastructure.IdentityVerification.Didit.Payloads.Records;

public record WebhookPayload(
    [property: JsonPropertyName("session_id")]
    string SessionId,

    [property: JsonPropertyName("status")]
    string Status,

    [property: JsonPropertyName("webhook_type")]
    string? WebhookType,

    [property: JsonPropertyName("vendor_data")]
    string? VendorData
    );