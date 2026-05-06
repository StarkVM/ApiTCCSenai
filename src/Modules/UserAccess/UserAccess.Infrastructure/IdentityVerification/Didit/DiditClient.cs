using System.Net;
using System.Net.Http.Json;

using Microsoft.Extensions.Options;

using UserAccess.Domain.Interfaces;
using UserAccess.Domain.Results;
using UserAccess.Domain.Senders;

using UserAccess.Infrastructure.IdentityVerification.Didit.Options;
using UserAccess.Infrastructure.IdentityVerification.Didit.Requests;
using UserAccess.Infrastructure.IdentityVerification.Didit.Responses;

namespace UserAccess.Infrastructure.IdentityVerification.Didit;

public class DiditClient : IIdentityVerificationProvider
{
    private readonly HttpClient _httpClient;
    private readonly DiditOptions _options;

    public DiditClient(
        HttpClient httpClient,
        IOptions<DiditOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }
    
    public async Task<CreateProviderIdentityVerificationSessionResult> CreateSessionAsync
        (CreateProviderIdentityVerificationSessionRequest request, CancellationToken cancellationToken)
    {
        var diditRequest = new DiditCreateSessionRequest(
            WorkflowId: _options.WorkflowId,
            VendorData: request.LocalSessionId.ToString(),
            Language: "pt-BR",
            ContactDetails: new(
                Email: request.Email,
                SendNotificationEmails: true,
                EmailLanguage:  "pt-BR"
                ),
           ExpectedDetails: new DiditExpectedDetails(
               FirstName: request.FirstName,
               LastName: request.LastName,
               DateOfBirth: request.BirthDate.ToString("yyyy-MM-dd"),
               Nationality: "BRA",
               IdCountry: "BRA",
               
               // ID = national identity document.
               // DL = driver's license.
               // / ID = documento nacional de identidade.
               // / DL = carteira de motorista.
               ExpectedDocumentTypes: ["ID", "DL"]
               )
        );

        var response = await _httpClient.PostAsJsonAsync("/v3/session/", diditRequest, cancellationToken);
        
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            throw new HttpRequestException("DIDIT_SESSION_BAD_REQUEST");
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new HttpRequestException("DIDIT_UNAUTHORIZED");
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new HttpRequestException("DIDIT_FORBIDDEN");
        }
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Unexpected status code: {(int)response.StatusCode} ({response.StatusCode})");
        }

        var diditResponse = await response.Content.ReadFromJsonAsync<DiditCreateSessionResponse>(cancellationToken);
        
        if (diditResponse is null)
        {
            throw new InvalidOperationException("DIDIT_EMPTY_RESPONSE");
        }
        
        var verificationUrl = diditResponse.VerificationUrl ?? diditResponse.Url;

        if (string.IsNullOrWhiteSpace(diditResponse.SessionId))
        {
            throw new InvalidOperationException("DIDIT_SESSION_ID_NOT_RETURNED");
        }

        if (string.IsNullOrWhiteSpace(verificationUrl))
        {
            throw new InvalidOperationException("DIDIT_VERIFICATION_URL_NOT_RETURNED");
        }

        return new CreateProviderIdentityVerificationSessionResult(
            ProviderSessionId:  diditResponse.SessionId,
            VerificationUrl: verificationUrl,
            SessionToken:  diditResponse.SessionToken,
            ProviderStatus:   diditResponse.Status
        );
    }
}