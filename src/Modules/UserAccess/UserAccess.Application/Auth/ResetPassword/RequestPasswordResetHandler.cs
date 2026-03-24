using Microsoft.Extensions.Logging;
using UserAccess.Domain.Senders;
using UserAccess.Application.Auth.ResetPassword.Records;
using UserAccess.Domain.Enums;
using UserAccess.Domain.Helpers;
using UserAccess.Domain.Interfaces;

namespace UserAccess.Application.Auth.ResetPassword;

public sealed class RequestPasswordResetHandler
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<RequestPasswordResetHandler> _logger;
    private readonly IClock _clock;
    private readonly IVerificationCodeRepository _verificationCodeRepository;
    private readonly IVerificationCodeSender _verificationCodeSender;

    public RequestPasswordResetHandler(
        IUserRepository userRepository,
        IVerificationCodeRepository verificationCodeRepository,
        ILogger<RequestPasswordResetHandler> logger,
        IClock clock,
        IVerificationCodeSender verificationCodeSender)
    {
        _userRepository = userRepository;
        _logger = logger;
        _verificationCodeRepository = verificationCodeRepository;
        _clock = clock;
        _verificationCodeSender = verificationCodeSender;
    }
    
    public async Task<RequestPasswordResetResult> HandleAsync(RequestPasswordResetCommand resetCommand, CancellationToken cancellationToken)
    {
        var email = resetCommand.Email?.Trim().ToLowerInvariant();
        
        var nouUtc = _clock.UtcNow;
        
        Validate(email);
        
        _logger.LogInformation("Starting forgot password flow");
        
        var user = await _userRepository.GetByEmailAsync(email!, cancellationToken);

        if (user is null)
        {
            _logger.LogInformation("User not found");
            return new RequestPasswordResetResult(false);
        }
        
        if (user.Status != UserStatus.Active)
        {
            _logger.LogInformation("Invalid user");
            return new RequestPasswordResetResult(false);
        }
        
        await _verificationCodeRepository.InvalidateActiveCodesAsync(user.Id, VerificationCodePurpose.PasswordReset, cancellationToken);
        
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