using Microsoft.Extensions.Logging;
using UserAccess.Application.Auth.Login.Records;
using UserAccess.Domain.Senders;
using UserAccess.Domain.Enums;
using UserAccess.Domain.Helpers;
using UserAccess.Domain.Interfaces;

namespace UserAccess.Application.Auth.Login;

public sealed class RequestNewLoginVerificationCodeHandler
{
    private readonly IVerificationCodeRepository _verificationCodeRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<RequestNewLoginVerificationCodeHandler> _logger;
    private readonly IVerificationCodeSender _verificationCodeSender;
    private readonly IUnitOfWork _unitOfWork;

    public RequestNewLoginVerificationCodeHandler(
        IUserRepository userRepository,
        IVerificationCodeRepository verificationCodeRepository,
        ILogger<RequestNewLoginVerificationCodeHandler> logger,
        IVerificationCodeSender verificationCodeSender,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _verificationCodeRepository = verificationCodeRepository;
        _logger = logger;
        _verificationCodeSender = verificationCodeSender;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<RequestNewLoginVerificationCodeResult> HandleAsync(RequestNewLoginVerificationCodeCommand command, CancellationToken cancellationToken)
    {
        var email = command.Email?.Trim().ToLowerInvariant();
        
        Validate(email);
        
        _logger.LogInformation("Starting request new login verification code flow");
        
        var user = await _userRepository.GetByEmailAsync(email!, cancellationToken);

        if (user is null)
        {
            _logger.LogInformation("User not found");
            return new RequestNewLoginVerificationCodeResult(false);
        }

        if (user.Status != UserStatus.Active)
        {
            _logger.LogInformation("User is not active. UserId: {UserId}", user.Id);
            return new RequestNewLoginVerificationCodeResult(false);
        }
        
        var verificationCode = await _verificationCodeRepository.GetLatestActiveAsync(
            user.Id,
            VerificationCodePurpose.LoginVerification,
            cancellationToken);

        if (verificationCode is null)
        {
            _logger.LogInformation("Valid code not found");
            return new RequestNewLoginVerificationCodeResult(false);
        }
        
        await _verificationCodeRepository.InvalidateActiveCodesAsync(
            user.Id,
            VerificationCodePurpose.LoginVerification,
            cancellationToken);
        
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
            VerificationCodePurpose.LoginVerification
        );

        try
        {
            await _verificationCodeSender.SendCodeAsync(senderEmailCommand, cancellationToken);
            _logger.LogInformation("Resend login verification code sent successfully for email {Email}", email);
        }
        catch (ArgumentException ex) when (ex.Message == "VERY_FAST_ATTEMPTS")
        {
            return new RequestNewLoginVerificationCodeResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resend login verification code for email {Email}", email);
            throw new InvalidOperationException("EMAIL_SEND_FAILED", ex);
        }
       
        return new RequestNewLoginVerificationCodeResult(true);
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