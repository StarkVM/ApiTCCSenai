using Microsoft.Extensions.Logging;
using UserAccess.Application.Auth.Login.Records;
using UserAccess.Domain.Senders;
using UserAccess.Domain.Enums;
using UserAccess.Domain.Helpers;
using UserAccess.Domain.Interfaces;

namespace UserAccess.Application.Auth.Login;

public sealed class LoginHandler
{
        private readonly IUserRepository _userRepository;
    private readonly ILogger<LoginHandler> _logger;
    private readonly IVerificationCodeSender _verificationCodeSender;
    private readonly IPasswordHasher _passwordHasher;

    public LoginHandler(
        IUserRepository userRepository,
        IVerificationCodeRepository verificationCodeRepository,
        ILogger<LoginHandler> logger,
        IClock clock,
        IVerificationCodeSender verificationCodeSender,
        IPasswordHasher passwordHasher
        )
    {
        _userRepository = userRepository;
        _logger = logger;
        _verificationCodeSender = verificationCodeSender;
        _passwordHasher = passwordHasher;
    }
    
    public async Task<LoginResult> HandleAsync(LoginCommand command, CancellationToken cancellationToken)
    {
        var email = command.Email?.Trim().ToLowerInvariant();
        var password = command.Password?.Trim();
        
        Validate(email, password);
        
        _logger.LogInformation("Starting login flow");
        
        var user = await _userRepository.GetByEmailAsync(email!, cancellationToken);

        if (user is null)
        {
            _logger.LogInformation("User not found");
            return new LoginResult(false);
        }
        
        if (user.Status != UserStatus.Active && user.Status != UserStatus.PendingIdentityVerification)//Add new status after new verification
        {
            _logger.LogInformation("Invalid user");
            return new LoginResult(false);
        }
        
        var result = _passwordHasher.Verify(password!, user.PasswordHash);

        if (!result)
        {
            _logger.LogInformation("Login failed because password is wrong for email {Email}", email);
            return new LoginResult(false);
        }
        
        var senderEmailCommand = new SendVerificationCodeRequest(
            email!,
            user.Id,
            VerificationCodePurpose.LoginVerification
        );
        
        try
        {
            await _verificationCodeSender.SendCodeAsync(senderEmailCommand, cancellationToken);
            _logger.LogInformation("Login verification code sent successfully for email {Email}", email);
        }
        catch (ArgumentException ex) when (ex.Message == "VERY_FAST_ATTEMPTS")
        {
            return new LoginResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email verification code for email {Email}", email);
            throw new InvalidOperationException("EMAIL_SEND_FAILED", ex);
        }
       
        return new LoginResult(true);
    }

    private static void Validate(string? email, string? password)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("EMAIL_IS_REQUIRED");
        }
        if (!email.EmailIsValid())
        {
            throw new ArgumentException("EMAIL_INVALID");
        }
        
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("PASSWORD_IS_REQUIRED");
        }
        if (password.Length < 8)
        {
            throw new ArgumentException("PASSWORD_INVALID_LENGTH");
        }
    }
}