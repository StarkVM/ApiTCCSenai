using System.Text.Json.Serialization;

namespace UserAccess.Infrastructure.IdentityVerification.Didit.Responses;

public sealed record DiditIdVerificationDecisionResponse
{
    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("first_name")]
    public string? FirstName { get; init; }

    [JsonPropertyName("last_name")]
    public string? LastName { get; init; }

    [JsonPropertyName("full_name")]
    public string? FullName { get; init; }

    [JsonPropertyName("date_of_birth")]
    public string? DateOfBirth { get; init; }

    [JsonPropertyName("document_number")]
    public string? DocumentNumber { get; init; }

    [JsonPropertyName("personal_number")]
    public string? PersonalNumber { get; init; }
}

public sealed record DiditSessionDecisionResponse
{
    [JsonPropertyName("session_id")]
    public string? SessionId { get; init; }

    [JsonPropertyName("session_kind")]
    public string? SessionKind { get; init; }

    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("id_verifications")]
    public List<DiditIdVerificationDecisionResponse>? IdVerifications { get; init; }
}
    