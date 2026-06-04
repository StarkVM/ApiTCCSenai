using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
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
    private readonly ICpfHasher _cpfHasher;
    private readonly HttpClient _httpClient;
    private readonly DiditOptions _options;
    private readonly ILogger<DiditClient> _logger;

    public DiditClient(
        HttpClient httpClient,
        ICpfHasher cpfHasher,
        IOptions<DiditOptions> options,
        ILogger<DiditClient> logger)
    {
        _cpfHasher  = cpfHasher;
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
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

    public async Task<VerifyProviderIdentityResult> VerifyIdentityAsync(VerifyProviderIdentityRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ProviderSessionId))
    {
        return new VerifyProviderIdentityResult(false, null, null);
    }

    using var response = await _httpClient.GetAsync(
        $"/v3/session/{Uri.EscapeDataString(request.ProviderSessionId)}/decision/",
        cancellationToken);

    if (response.StatusCode == HttpStatusCode.NotFound)
    {
        throw new HttpRequestException("DIDIT_SESSION_DECISION_NOT_FOUND");
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
        throw new HttpRequestException(
            $"Unexpected status code: {(int)response.StatusCode} ({response.StatusCode})");
    }

    var diditDecision =
        await response.Content.ReadFromJsonAsync<DiditSessionDecisionResponse>(
            cancellationToken);

    if (diditDecision is null)
    {
        throw new InvalidOperationException("DIDIT_DECISION_EMPTY_RESPONSE");
    }

    if (!string.Equals(
            diditDecision.SessionId,
            request.ProviderSessionId,
            StringComparison.Ordinal))
    {
        _logger.LogInformation(
            "111");
        
        return new VerifyProviderIdentityResult(false, null, null);
    }

    if (!string.Equals(
            diditDecision.SessionKind,
            "user",
            StringComparison.OrdinalIgnoreCase))
    {
        _logger.LogInformation(
            "222");
        return new VerifyProviderIdentityResult(false, null, null);
    }

    if (!ProviderStatusIsApproved(diditDecision.Status))
    {
        _logger.LogInformation(
            "333");
        return new VerifyProviderIdentityResult(false, null, null);
    }

    var approvedIdVerification = diditDecision.IdVerifications?
        .FirstOrDefault(verification =>
            ProviderStatusIsApproved(verification.Status));

    if (approvedIdVerification is null)
    {
        _logger.LogInformation(
            "444");
        return new VerifyProviderIdentityResult(false, null, null);
    }

    var nameMatches = DocumentNameMatches(
        expectedFirstName: request.ExpectedFirstName,
        expectedLastName: request.ExpectedLastName,
        diditVerification: approvedIdVerification);

    var birthDateMatches = BirthDateMatches(
        expectedBirthDate: request.ExpectedBirthDate,
        diditBirthDate: approvedIdVerification.DateOfBirth);

    var cpfMatches = CpfMatches(
        expectedCpfHash: request.ExpectedCpfHash,
        diditVerification: approvedIdVerification);

    var identityIsValid =
        nameMatches &&
        birthDateMatches &&
        cpfMatches;
    
    _logger.LogInformation(
        "Cpf: {cpf}, Data: {data}, Nome: {nome}", cpfMatches, birthDateMatches, nameMatches);

    return new VerifyProviderIdentityResult(identityIsValid, null, null);
    
    }
    
    private static bool ProviderStatusIsApproved(string? status)
    {
        return NormalizeProviderStatus(status) == "APPROVED";
    }
    
    private static string NormalizeProviderStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return string.Empty;
        }

        var lettersAndDigitsOnly = new string(
            status
                .Where(char.IsLetterOrDigit)
                .ToArray());

        return lettersAndDigitsOnly.ToUpperInvariant();
    }
    
    private static bool DocumentNameMatches(
        string expectedFirstName,
        string expectedLastName,
        DiditIdVerificationDecisionResponse diditVerification)
    {
        var expectedFullName = NormalizeName(
            $"{expectedFirstName} {expectedLastName}");

        if (!string.IsNullOrWhiteSpace(diditVerification.FullName))
        {
            var receivedFullName = NormalizeName(
                diditVerification.FullName);

            return string.Equals(
                expectedFullName,
                receivedFullName,
                StringComparison.Ordinal);
        }

        if (string.IsNullOrWhiteSpace(diditVerification.FirstName) ||
            string.IsNullOrWhiteSpace(diditVerification.LastName))
        {
            return false;
        }

        var expectedFirstNameNormalized = NormalizeName(expectedFirstName);
        var expectedLastNameNormalized = NormalizeName(expectedLastName);

        var receivedFirstNameNormalized = NormalizeName(diditVerification.FirstName);
        var receivedLastNameNormalized = NormalizeName(diditVerification.LastName);
        
        var expectedFullNameNormalized = $"{expectedFirstNameNormalized} {expectedLastNameNormalized}";
        var receivedFullNameNormalized = $"{receivedFirstNameNormalized} {receivedLastNameNormalized}";

        return string.Equals(
            expectedFullNameNormalized,
            receivedFullNameNormalized,
            StringComparison.Ordinal);
    }
    private static string NormalizeName(string name)
    {
        var normalized = name
            .Trim()
            .Normalize(NormalizationForm.FormD);

        var withoutAccents = new string(
            normalized
                .Where(character =>
                    CharUnicodeInfo.GetUnicodeCategory(character)
                    != UnicodeCategory.NonSpacingMark)
                .ToArray());

        var recomposed = withoutAccents
            .Normalize(NormalizationForm.FormC);

        var collapsedSpaces = Regex.Replace(
            recomposed,
            @"\s+",
            " ");

        return collapsedSpaces
            .Trim()
            .ToUpperInvariant();
    }
    
    private static bool BirthDateMatches(
        DateOnly expectedBirthDate,
        string? diditBirthDate)
    {
        if (string.IsNullOrWhiteSpace(diditBirthDate))
        {
            return false;
        }

        var parsed = DateOnly.TryParseExact(
            diditBirthDate,
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var parsedBirthDate);

        if (!parsed)
        {
            return false;
        }

        return parsedBirthDate == expectedBirthDate;
    }
    
    private bool CpfMatches(
        string expectedCpfHash,
        DiditIdVerificationDecisionResponse diditVerification)
    {
        var cpfCandidates = new[]
        {
            diditVerification.PersonalNumber,
            diditVerification.DocumentNumber
        };

        foreach (var candidate in cpfCandidates)
        {
            
            var cleanCpf = ExtractCleanCpfOrNull(candidate);

            if (cleanCpf is null)
            {
                continue;
            }

            if (_cpfHasher.Verify(cleanCpf, expectedCpfHash))
            {
                return true;
            }
        }

        return false;
    }
    
    private static string? ExtractCleanCpfOrNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var digitsOnly = new string(
            value
                .Where(char.IsDigit)
                .ToArray());

        if (digitsOnly.Length == 11)
        {
            return digitsOnly;
        }
        else
        {
            return null;
        }
    }
}