using System.Text.Json.Serialization;

namespace UserAccess.Infrastructure.IdentityVerification.Didit.Requests;

public record DiditCreateSessionRequest(
    [property: JsonPropertyName("workflow_id")]
    string WorkflowId,

    [property: JsonPropertyName("vendor_data")]
    string VendorData,

    [property: JsonPropertyName("language")]
    string Language,

    [property: JsonPropertyName("contact_details")]
    DiditContactDetails ContactDetails,

    [property: JsonPropertyName("expected_details")]
    DiditExpectedDetails ExpectedDetails
    );
    
public record DiditExpectedDetails(
    [property: JsonPropertyName("first_name")]
    string FirstName,

    [property: JsonPropertyName("last_name")]
    string LastName,

    [property: JsonPropertyName("date_of_birth")]
    string DateOfBirth,

    [property: JsonPropertyName("nationality")]
    string Nationality,

    [property: JsonPropertyName("id_country")]
    string IdCountry,

    [property: JsonPropertyName("expected_document_types")]
    string[] ExpectedDocumentTypes
);

public record DiditContactDetails(
    [property: JsonPropertyName("email")]
    string Email,

    [property: JsonPropertyName("send_notification_emails")]
    bool SendNotificationEmails,

    [property: JsonPropertyName("email_lang")]
    string EmailLanguage
);