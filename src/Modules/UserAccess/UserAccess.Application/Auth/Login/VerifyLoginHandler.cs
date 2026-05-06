using System.Security.Authentication;
using UserAccess.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using UserAccess.Application.Auth.Login.Records;
using UserAccess.Application.Common.Exceptions;
using UserAccess.Domain.Enums;
using UserAccess.Domain.Exceptions.UserAccessExceptions;
using UserAccess.Domain.Helpers;

namespace UserAccess.Application.Auth.Login;

public sealed class VerifyLoginHandler
{
     private readonly IUserRepository _userRepository;
    private readonly IVerificationCodeService _verificationCodeService;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenIssuer _tokenIssuer;
    private readonly ILogger<VerifyLoginHandler> _logger;
    public VerifyLoginHandler(
        IUserRepository userRepository,
        IVerificationCodeService verificationCodeService,
        IClock clock,
        IUnitOfWork unitOfWork,
        ITokenIssuer tokenIssuer,
        ILogger<VerifyLoginHandler> logger)
    {
        _userRepository = userRepository;
        _verificationCodeService = verificationCodeService;
        _clock = clock;
        _unitOfWork =  unitOfWork;
        _tokenIssuer = tokenIssuer;
        _logger = logger;
    }

    public async Task<VerifyLoginResult> HandleAsync(VerifyLoginCommand command, CancellationToken cancellationToken)
    {
        // Normalização básica
        // Basic normalization
        var email = command.Email?.Trim().ToLowerInvariant();
        var code = command.Code?.Trim();
        
        var utcNow = _clock.UtcNow;
        
        _logger.LogInformation("Starting verify login by code flow");
        
        Validate(email, code);
        
        // Busca usuário
        // Find user by email
        var user = await _userRepository.GetByEmailAsync(email!, cancellationToken);
        

        if (user is null )
        {
            _logger.LogWarning("Login verification failed: user not found for email {Email}.", email);
            // Não revelar que o usuário não existe
            // Do not reveal user existence
            throw new InvalidCredentialsException();
        }
        
        if (user.Status != UserStatus.Active && user.Status != UserStatus.PendingIdentityVerification)
        {
            _logger.LogWarning("User {Email} is not Active.", email);
            // Não revelar que o usuário existe mas nao eh valido
            // Do not reveal that user exist but is not valid
            throw new InvalidCredentialsException();
        }

        var validation = await _verificationCodeService.ValidateAsync(
            user,
            code!,
            VerificationCodePurpose.LoginVerification,
            cancellationToken
        );
    
        if (!validation.IsValid || validation.Code is null)
        {
            _logger.LogWarning("Invalid login verification code for email {Email}.", email);
            //Invalid code
            throw new InvalidCredentialsException();
        }

        var verificationCode = validation.Code;
        
        verificationCode.Consume(utcNow);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Login verification completed successfully for email {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist login verification for email {Email}.", email);
            throw new DatabaseSaveFailedException(ex);
        }

        var tokens = await _tokenIssuer.IssueAsync(user, cancellationToken);

        return new VerifyLoginResult(
            tokens.AccessToken,
            tokens.RefreshToken,
            tokens.AccessTokenExpiresAtUtc,
            tokens.RefreshTokenExpiresAtUtc
        );
    }

    private static void Validate(string? email, string? code)
    {
        //Email validation
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.");
        }

        if (!email.EmailIsValid())
        {
            throw new ArgumentException("Invalid email format.");
        }
        //Code validation
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Verification code is required.");
        }
        if (code.Length != 6)
        {
            throw new ArgumentException("Verification code must be exactly 6 characters.");
        }
    }
}