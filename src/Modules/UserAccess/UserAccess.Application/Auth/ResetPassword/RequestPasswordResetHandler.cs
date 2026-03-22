using Microsoft.Extensions.Logging;
using UserAccess.Domain.Senders;
using UserAccess.Application.Auth.ResetPassword.Records;
using UserAccess.Domain.Enums;
using UserAccess.Domain.Helpers;
using UserAccess.Domain.Interfaces;
using UserAccess.Application.Auth.VerificationCodes;


namespace UserAccess.Application.Auth.ResetPassword;

public sealed class RequestPasswordResetHandler
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<RequestPasswordResetHandler> _logger;
    private readonly IClock _clock;
    private readonly IVerificationCodeSender _verificationCodeSender;

    public RequestPasswordResetHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILogger<RequestPasswordResetHandler> logger,
        IClock clock,
        IVerificationCodeSender verificationCodeSender)
    {
        _userRepository = userRepository;
        _logger = logger;
        _clock = clock;
        _verificationCodeSender = verificationCodeSender;
    }
    
    public async Task<RequestPasswordResetResult> HandleAsync(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var email = command.Email?.Trim().ToLowerInvariant();
        
        var nouUtc = _clock.UtcNow;
        
        Validate(email);
        
        _logger.LogInformation("Starting forgot password flow");
        
        var user = await _userRepository.GetByEmailAsync(email!, cancellationToken);

        if (user is null)
        {
            _logger.LogError("User not found");
            return new RequestPasswordResetResult(false);
        }

        var senderEmailCommand = new SendVerificationCodeRequest(
            email!,
            user.Id,
            VerificationCodePurpose.PasswordReset
        );
        
        try
        {
            await _verificationCodeSender.SendCodeAsync(senderEmailCommand, cancellationToken);
            _logger.LogInformation("Reset password code sent successfully for email {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send reset password code for email {Email}", email);
            throw new InvalidOperationException("EMAIL_SEND_FAILED", ex);
        }
       
        return new RequestPasswordResetResult(true);
    }

    private static void Validate(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("EMAIL_IS_REQUIRED");
        }
        if (!email.EmailIsValid())
        {
            throw new ArgumentException("EMAIL_INVALID");
        }
    }
}