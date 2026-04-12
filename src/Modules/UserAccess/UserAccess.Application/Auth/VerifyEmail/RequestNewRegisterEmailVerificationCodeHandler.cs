using Microsoft.Extensions.Logging;
using UserAccess.Domain.Senders;
using UserAccess.Application.Auth.VerifyEmail.Records;
using UserAccess.Domain.Enums;
using UserAccess.Domain.Helpers;
using UserAccess.Domain.Interfaces;

namespace UserAccess.Application.Auth.VerifyEmail;

public sealed class RequestNewRegisterEmailVerificationCodeHandler
{
    private readonly IVerificationCodeRepository _verificationCodeRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<RequestNewRegisterEmailVerificationCodeHandler> _logger;
    private readonly IVerificationCodeSender _verificationCodeSender;
    private readonly IUnitOfWork _unitOfWork;

    public RequestNewRegisterEmailVerificationCodeHandler(
        IUserRepository userRepository,
        IVerificationCodeRepository verificationCodeRepository,
        ILogger<RequestNewRegisterEmailVerificationCodeHandler> logger,
        IVerificationCodeSender verificationCodeSender,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _verificationCodeRepository = verificationCodeRepository;
        _logger = logger;
        _verificationCodeSender = verificationCodeSender;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<RequestNewRegisterEmailVerificationCodeResult> HandleAsync(RequestNewRegisterEmailVerificationCodeCommand newCommand, CancellationToken cancellationToken)
    {
        var email = newCommand.Email?.Trim().ToLowerInvariant();
        
        
        Validate(email);
        
        _logger.LogInformation("Starting request new email verification code flow");
        
        var user = await _userRepository.GetByEmailAsync(email!, cancellationToken);

        if (user is null)
        {
            _logger.LogInformation("User not found");
            return new RequestNewRegisterEmailVerificationCodeResult(false);
        }

        if (user.Status != UserStatus.PendingEmailVerification)
        {
            _logger.LogInformation("User is not pending email verification. UserId: {UserId}", user.Id);
            return new RequestNewRegisterEmailVerificationCodeResult(false);
        }
        
        var verificationCode = await _verificationCodeRepository.GetLatestActiveAsync(
            user.Id,
            VerificationCodePurpose.EmailVerification,
            cancellationToken);

        if (verificationCode is null)
        {
            _logger.LogInformation("Valid code not found");
            return new RequestNewRegisterEmailVerificationCodeResult(false);
        }
        
        await _verificationCodeRepository.InvalidateActiveCodesAsync(user.Id, VerificationCodePurpose.EmailVerification, cancellationToken);
        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Data saved successfully for email {Email}", email);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to save data for email {Email}", email);
            throw new InvalidOperationException("DB_SAVE_FAILED", ex);
        }
        
        var senderEmailCommand = new SendVerificationCodeRequest(
            email!,
            user.Id,
            VerificationCodePurpose.EmailVerification
        );
        
        try
        {
            await _verificationCodeSender.SendCodeAsync(senderEmailCommand, cancellationToken);
            _logger.LogInformation("Resend email verification code sent successfully for email {Email}", email);
        }
        catch (ArgumentException ex) when (ex.Message == "VERY_FAST_ATTEMPTS")
        {
            return new RequestNewRegisterEmailVerificationCodeResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resend email verification code for email {Email}", email);
            throw new InvalidOperationException("EMAIL_SEND_FAILED", ex);
        }
       
        return new RequestNewRegisterEmailVerificationCodeResult(true);
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