using Microsoft.Extensions.Logging;
using UserAccess.Application.Common.Exceptions;
using UserAccess.Application.IdentityVerification.CreateIdentityVerificationSession.Records;
using UserAccess.Domain.Entities;
using UserAccess.Domain.Enums;
using UserAccess.Domain.Exceptions.UserAccessExceptions;
using UserAccess.Domain.Interfaces;
using UserAccess.Domain.Senders;

namespace UserAccess.Application.IdentityVerification.CreateIdentityVerificationSession;

/// <summary>
/// Handles the identity verification session creation flow.
/// / Manipula o fluxo de criação da sessão de verificação de identidade.
/// </summary>
public sealed class CreateIdentityVerificationSessionHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IIdentityVerificationRepository _sessionRepository;
    private readonly IIdentityVerificationProvider _identityVerificationProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly ILogger<CreateIdentityVerificationSessionHandler> _logger;
    
    public CreateIdentityVerificationSessionHandler(
        IUserRepository userRepository,
        IIdentityVerificationRepository sessionRepository,
        IIdentityVerificationProvider identityVerificationProvider,
        IUnitOfWork unitOfWork,
        IClock clock,
        ILogger<CreateIdentityVerificationSessionHandler> logger
        )
    {
        _userRepository = userRepository;
        _sessionRepository = sessionRepository;
        _identityVerificationProvider = identityVerificationProvider;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _logger = logger;
    }

    public async Task<CreateIdentityVerificationSessionResult> HandleAsync(
        CreateIdentityVerificationSessionCommand command,
        CancellationToken cancellationToken
        )
    {
        var nowUtc = _clock.UtcNow;
        var user = await _userRepository.GetByIdAsync(command.Id, cancellationToken);

        if (user is null)
        {
            throw new UserNotFoundException();
        }

        if (user.Status != UserStatus.PendingIdentityVerification)
        {
            throw new InvalidCredentialsException();
        }
        
        var existingSession = await _sessionRepository.GetLatestByUserIdAsync(user.Id, cancellationToken);

        if (existingSession is not null && !string.IsNullOrWhiteSpace(existingSession.ProviderSessionUrl))
        {
            _logger.LogInformation(
                "Reusing pending identity verification session. UserId: {UserId}, LocalSessionId: {LocalSessionId}",
                user.Id,
                existingSession.Id);

            return new CreateIdentityVerificationSessionResult(existingSession.ProviderSessionUrl);
        }

        var localSession = new IdentityVerificationSession(
            Guid.NewGuid(),
            user.Id,
            IdentityVerificationProvider.Didit,
            nowUtc
        );

        var providerRequest = new CreateProviderIdentityVerificationSessionRequest(
            localSession.Id,
            user.FirstName,
            user.LastName,
            user.BirthDate,
            user.Email
        );

        try
        {
            var providerResult = await _identityVerificationProvider.CreateSessionAsync(providerRequest, cancellationToken);

            localSession.AttachProviderSession(providerResult.ProviderSessionId, providerResult.VerificationUrl);
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation(
                "Registration data saved successfully for UserId: {UserId}, LocalSessionId: {LocalSessionId}",
                user.Id,
                localSession.Id);
             
            return new CreateIdentityVerificationSessionResult(providerResult.VerificationUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "Registration data saved failed for UserId: {UserId}, LocalSessionId: {LocalSessionId}",
                user.Id,
                localSession.Id);
            throw new DatabaseSaveFailedException(ex);
        }
    }
}