using Microsoft.Extensions.Logging;
using UserAccess.Application.Common.Exceptions;
using UserAccess.Application.IdentityVerification.ProcessIdentityVerificationWebhook.Records;
using UserAccess.Domain.Enums;
using UserAccess.Domain.Exceptions.UserAccessExceptions;
using UserAccess.Domain.Results;
using UserAccess.Domain.Interfaces;
using UserAccess.Domain.Senders;

namespace UserAccess.Application.IdentityVerification.ProcessIdentityVerificationWebhook;

public class ProcessIdentityVerificationWebhookHandler
{
    private readonly IIdentityVerificationProvider _identityVerificationProvider;
    private readonly IIdentityVerificationWebhookAuthenticator _webhookAuthenticator;
    private readonly IIdentityVerificationRepository _sessionRepository;
    private readonly IIdentityVerificationWebhookParser _webhookParser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;
    private readonly IClock _clock;
    private readonly ILogger<ProcessIdentityVerificationWebhookHandler> _logger;

    public ProcessIdentityVerificationWebhookHandler(
        IIdentityVerificationRepository sessionRepository,
        IUnitOfWork unitOfWork,
        IUserRepository userRepository,
        IIdentityVerificationWebhookParser webhookParser,
        IClock clock,
        IIdentityVerificationProvider identityVerificationProvider,
        IIdentityVerificationWebhookAuthenticator webhookAuthenticator,
        ILogger<ProcessIdentityVerificationWebhookHandler> logger)
    {
        _sessionRepository = sessionRepository;
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _webhookParser = webhookParser;
        _webhookAuthenticator = webhookAuthenticator;
        _identityVerificationProvider = identityVerificationProvider;
        _clock = clock;
        _logger = logger;
    }

    public async Task<ProcessIdentityVerificationWebhookResult> HandleAsync(
        ProcessIdentityVerificationWebhookCommand command,
        CancellationToken cancellationToken)
    {

        if (string.IsNullOrWhiteSpace(command.RawBody))
        {
            throw new WebhookInvalidPayloadException();
        }

        var isAuthentic = await _webhookAuthenticator.IsAuthentic(
            rawBody: command.RawBody,
            signatureV2: command.SignatureV2,
            signatureSimple: command.SignatureSimple,
            timestamp: command.Timestamp);

        if (!isAuthentic)
        {
            _logger.LogWarning("Invalid Didit webhook signature.");

            throw new WebhookInvalidSignatureException();
        }

        IdentityVerificationWebhookParserResult payload;

        try
        {
            payload = _webhookParser.Parse(command.RawBody);
        }
        catch
        {
            throw new WebhookInvalidPayloadException();
        }

        if (string.IsNullOrWhiteSpace(payload.ProviderStatus) ||
            string.IsNullOrWhiteSpace(payload.ProviderSessionId) ||
            string.IsNullOrWhiteSpace(payload.VendorData) ||
            string.IsNullOrWhiteSpace(payload.ProviderEventType))
        {
            throw new WebhookInvalidPayloadException();
        }

        if (!Guid.TryParse(payload.VendorData, out Guid vendorData))
        {
            throw new InvalidGuidIdException();
        }
        
        if (!payload.ProviderEventType.Equals("status.updated", StringComparison.Ordinal))
        {
            return new ProcessIdentityVerificationWebhookResult(
                Success: true,
                Code: "DIDIT_WEBHOOK_EVENT_TYPE_IGNORED");
        }

        var session = await _sessionRepository.GetByIdAsync(vendorData, cancellationToken);

        if (session is null)
        {
            _logger.LogWarning(
                "Didit webhook received for unknown session. VendorData: {VendorData}",
                payload.VendorData);

            return new ProcessIdentityVerificationWebhookResult(
                Success: true,
                Code: "DIDIT_WEBHOOK_UNKNOWN_SESSION_IGNORED");
        }
        
        if (!string.Equals(session.ProviderSessionId, payload.ProviderSessionId, StringComparison.Ordinal))
        {
            _logger.LogWarning(
                "Didit webhook provider session id mismatch. LocalSessionId: {LocalSessionId}",
                session.Id);

            throw new WebhookInvalidPayloadException();
        }
        
        if (session.Status != IdentityVerificationStatus.Pending)
        {
            return new ProcessIdentityVerificationWebhookResult(
                Success: true,
                Code: "DIDIT_WEBHOOK_ALREADY_PROCESSED"
            );
        }

        if (payload.ProviderStatus.Equals("Approved", StringComparison.Ordinal))
        {
            return await ProcessApprovedAsync(session, cancellationToken);
        }
        
        return await ProcessAsync(
            session,
            payload,
            cancellationToken);
    }

    private async Task<ProcessIdentityVerificationWebhookResult> ProcessApprovedAsync(
        Domain.Entities.IdentityVerificationSession session,
        CancellationToken cancellationToken)
    {
        var nowUtc = _clock.UtcNow;
        
        if (string.IsNullOrWhiteSpace(session.ProviderSessionId))
        {
            session.MarkFailed(_clock.UtcNow);
            
            await SaveChangesAsync(cancellationToken);
            
            return new ProcessIdentityVerificationWebhookResult(
                Success: true,
                Code: "PROVIDER_SESSION_ID_MISSING");
        }

        var user = await _userRepository.GetByIdAsync(session.UserId, cancellationToken);

        if (user is null)
        {
            session.MarkFailed(_clock.UtcNow);
            
            await SaveChangesAsync(cancellationToken);

            return new ProcessIdentityVerificationWebhookResult(
                Success: true,
                Code: "USER_NOT_FOUND");
        }
        var request = new 
            VerifyProviderIdentityRequest(
                session.ProviderSessionId,
                user.FirstName,
                user.LastName,
                user.BirthDate,
                user.CpfHash
                );
            
        var result = await _identityVerificationProvider.VerifyIdentityAsync(request,cancellationToken);

        if (!result.IsValid)
        {
            user.MarkIdentityDenied();
            session.MarkDenied(_clock.UtcNow);
            
            await SaveChangesAsync(cancellationToken);
            
            return new ProcessIdentityVerificationWebhookResult(
                Success: true,
                Code: "USER_NOT_VALID");
        }
        
        user.ActivateUser();
        session.MarkApproved(nowUtc);

        await SaveChangesAsync(cancellationToken);
        
        return new ProcessIdentityVerificationWebhookResult(
            Success: true,
            Code: "IDENTITY_VERIFICATION_APPROVED");
    }
    
    private async Task<ProcessIdentityVerificationWebhookResult> ProcessAsync(
        Domain.Entities.IdentityVerificationSession session,
        IdentityVerificationWebhookParserResult payload,
        CancellationToken cancellationToken)
    {
        if (payload.ProviderStatus.Equals("Declined", StringComparison.Ordinal))
        {
            session.MarkFailed(_clock.UtcNow);
        }

        if (payload.ProviderStatus.Equals("Expired", StringComparison.Ordinal) ||
            payload.ProviderStatus.Equals("Kyc Expired", StringComparison.Ordinal))
        {
            session.MarkExpired(_clock.UtcNow);
        }

        if (payload.ProviderStatus.Equals("Abandoned", StringComparison.Ordinal))
        {
            session.MarkFailed(_clock.UtcNow);
        }

        await SaveChangesAsync(cancellationToken);

        return new ProcessIdentityVerificationWebhookResult(
            Success: true,
            Code: "IDENTITY_VERIFICATION_NOT_APPROVED_BY_PROVIDER");
    }
    
    private async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new DatabaseSaveFailedException(ex);
        }
    }
}

