using UserAccess.Application.Auth.VerifyEmail.Records;
using UserAccess.Domain.Entities;
using UserAccess.Domain.Interfaces;
using UserAccess.Domain.Helpers;

namespace UserAccess.Application.Auth.VerifyEmail;

public class SendEmailVerificationCode
{
    private readonly IEmailVerificationRepository _emailVerificationRepository;
    private readonly IEmailSender _emailSender;
    private readonly IVerificationCodeHasher _verificationCodeHasher;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;

    public SendEmailVerificationCode(
        IEmailVerificationRepository emailVerificationRepository,
        IEmailSender emailSender,
        IVerificationCodeHasher verificationCodeHasher,
        IClock clock,
        IUnitOfWork unitOfWork)
    {
        _emailVerificationRepository = emailVerificationRepository;
        _emailSender = emailSender;
        _verificationCodeHasher = verificationCodeHasher;
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(SenderEmailCommand command, CancellationToken cancellationToken)
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
            expiresAt
        );
        
        await _emailVerificationRepository.AddAsync( emailVerificationCode, cancellationToken);

        var subject = "Verify your email – your code is inside";

        var body = $"""
                    Hello,
                   
                   Thanks for signing up.
                   
                   Your verification code is:
                   
                   {code}
                   
                   This code will expire in 5 minutes.
                   
                   If you didn’t request this, you can safely ignore this email.
                   
                   For security reasons, do not share this code with anyone.
                   
                   HeavyRent — Team
                   """;

        await _emailSender.SendAsync(email, subject, body, cancellationToken );
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}