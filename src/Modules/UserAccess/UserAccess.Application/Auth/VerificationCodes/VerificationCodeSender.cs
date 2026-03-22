using UserAccess.Domain.Senders;
using UserAccess.Domain.Entities;
using UserAccess.Domain.Enums;
using UserAccess.Domain.Interfaces;
using UserAccess.Domain.Helpers;

namespace UserAccess.Application.Auth.VerificationCodes;

public sealed class VerificationCodeSender : IVerificationCodeSender
{
    private readonly IVerificationCodeRepository _verificationCodeRepository;
    private readonly IEmailSender _emailSender;
    private readonly IVerificationCodeHasher _verificationCodeHasher;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;

    public VerificationCodeSender(
        IVerificationCodeRepository verificationCodeRepository,
        IEmailSender emailSender,
        IVerificationCodeHasher verificationCodeHasher,
        IClock clock,
        IUnitOfWork unitOfWork)
    {
        _verificationCodeRepository = verificationCodeRepository;
        _emailSender = emailSender;
        _verificationCodeHasher = verificationCodeHasher;
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    public async Task SendCodeAsync(SendVerificationCodeRequest command, CancellationToken cancellationToken)
    {
        var userId = command.UserId;
        var email = command.Email;
        
        var nowUtc = _clock.UtcNow;

        var code = Email.Code();

        var codeHash = _verificationCodeHasher.Hash(code);
        
        var expiresAt = nowUtc.AddMinutes(5);
        
        var emailVerificationCode = new EmailVerificationCode(
            Guid.NewGuid(),
            userId,
            codeHash,
            nowUtc,
            expiresAt,
            command.Purpose
        );
        
        await _verificationCodeRepository.AddAsync( emailVerificationCode, cancellationToken);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        string subject;

        string body;
        
        if (command.Purpose == VerificationCodePurpose.EmailVerification)
        {
            subject = "Verify your email – your code is inside";

            body = $"""
                        Hello,

                        Thanks for signing up.

                        Your verification code is:

                        {code}

                        This code will expire in 5 minutes.

                        If you didn’t request this, you can safely ignore this email.

                        For security reasons, do not share this code with anyone.

                        HeavyRent — Team
                        """;
        }
        else
        {
            subject = "Reset your password – your code is inside";

            body = $"""
                        Hello,

                        We received a request to reset your password.

                        Your password reset code is:

                        {code}

                        This code will expire in 5 minutes.

                        If you did not request a password reset, you can safely ignore this email. Your account will remain secure.

                        For security reasons, do not share this code with anyone.

                        HeavyRent — Team
                        """;
        }

        await _emailSender.SendAsync(email, subject, body, cancellationToken );
    }
}