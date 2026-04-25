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
        catch (Exception)
        {
            _logger.LogInformation("Login verification code sent failed for email {Email}", email);
            return new LoginResult(false);
        }
       
        return new LoginResult(true);
    }

    private static void Validate(string? email, string? password)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.");
        }
        if (!email.EmailIsValid())
        {
            throw new ArgumentException("Email format is invalid.");
        }
        
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password is required.");
        }
        if (password.Length < 8)
        {
            throw new ArgumentException("Password must be at least 8 characters long.");
        }
    }
}