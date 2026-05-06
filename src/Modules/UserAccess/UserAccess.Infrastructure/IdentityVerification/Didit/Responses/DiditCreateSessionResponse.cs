using System.Text.Json.Serialization;

namespace UserAccess.Infrastructure.IdentityVerification.Didit.Responses;

public class DiditCreateSessionResponse
{
    [JsonPropertyName("session_id")]
    public string SessionId { get; init; } = string.Empty;

    [JsonPropertyName("session_token")]
    public string SessionToken { get; init; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    [JsonPropertyName("url")]
    public string? Url { get; init; }

    [JsonPropertyName("verification_url")]
    public string? VerificationUrl { get; init; }
}