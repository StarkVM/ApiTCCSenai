using UserAccess.Application.Common.Exceptions;
using UserAccess.Domain.Senders;
using UserAccess.Domain.Entities;
using UserAccess.Domain.Enums;
using UserAccess.Domain.Exceptions.UserAccessExceptions;
using UserAccess.Domain.Interfaces;
using UserAccess.Domain.Helpers;

namespace UserAccess.Application.Auth.Services.VerificationCodesServices;

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
        
        var utcNow = _clock.UtcNow;
        
        var independentCode = await _verificationCodeRepository.GetLatestAsync(userId, command.Purpose, cancellationToken);
        
        if (independentCode is not null)
        {
            if (independentCode.CreatedAt > utcNow.AddMinutes(-1))
            {
                throw new TooManyAttemptsException();
            }
        }
        
        var code = Email.Code();

        var codeHash = _verificationCodeHasher.Hash(code);
        
        var expiresAt = utcNow.AddMinutes(5);
        
        var emailVerificationCode = new EmailVerificationCode(
            Guid.NewGuid(),
            userId,
            codeHash,
            utcNow,
            expiresAt,
            command.Purpose
        );
        
        await _verificationCodeRepository.AddAsync( emailVerificationCode, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            throw new DatabaseSaveFailedException(exception);
        }
        
        

        var subject = """ """;

        var body= """ """;
        
        if (command.Purpose == VerificationCodePurpose.EmailVerification)
        {
            subject = "Verify your email – your code is inside";

            body = $"""
                    <div style="font-family: Arial, sans-serif; background-color: #f5f5f5; padding: 20px;">
                      
                      <div style="max-width: 500px; margin: 0 auto; background-color: #ffffff; padding: 24px; border-radius: 8px;">
                        
                        <h2 style="margin-top: 0;">Verify your email</h2>

                        <p>Hello,</p>

                        <p>Thanks for signing up to <strong>HeavyRent</strong>.</p>

                        <p>Your verification code is:</p>

                        <div style="
                            font-size: 28px;
                            font-weight: bold;
                            letter-spacing: 6px;
                            text-align: center;
                            margin: 20px 0;
                        ">
                            {code}
                        </div>

                        <p>This code will expire in <strong>5 minutes</strong>.</p>

                        <p style="color: #555;">
                          If you didn’t request this, you can safely ignore this email.
                        </p>

                        <p style="font-size: 12px; color: #888;">
                          For security reasons, do not share this code with anyone.
                        </p>

                        <hr style="margin: 24px 0; border: none; border-top: 1px solid #eee;" />

                        <p style="font-size: 12px; color: #888; text-align: center;">
                          — HeavyRent Team
                        </p>

                      </div>

                    </div>
                    """;
        }
        else if (command.Purpose == VerificationCodePurpose.PasswordReset)
        {
            subject = "Reset your password – your code is inside";

            body = body = $"""
                           <div style="font-family: Arial, sans-serif; padding: 20px;">
                             <h2>Reset your password</h2>

                             <p>We received a request to reset your password.</p>

                             <p>Your password reset code is:</p>

                             <div style="font-size: 28px; font-weight: bold; letter-spacing: 4px; margin: 20px 0;">
                               {code}
                             </div>

                             <p>This code will expire in 5 minutes.</p>

                             <p style="color: gray;">
                               If you didn’t request this, you can safely ignore this email.
                             </p>

                             <p style="font-size: 12px; color: gray;">
                               For security reasons, never share this code with anyone.
                             </p>

                             <br/>

                             <p>— HeavyRent Team</p>
                           </div>
                           """;
        }
        else
        {
            subject = "Verify your email – your code is inside";
            
            body = $"""
                    <div style="font-family: Arial, sans-serif; background-color: #f5f5f5; padding: 20px;">
                      
                      <div style="max-width: 500px; margin: 0 auto; background-color: #ffffff; padding: 24px; border-radius: 8px;">
                        
                        <h2 style="margin-top: 0;">Verify your login</h2>

                        <p>Hello,</p>

                        <p>We detected a login attempt to your <strong>HeavyRent</strong> account.</p>

                        <p>Use the verification code below to continue:</p>

                        <div style="
                            font-size: 28px;
                            font-weight: bold;
                            letter-spacing: 6px;
                            text-align: center;
                            margin: 20px 0;
                        ">
                            {code}
                        </div>

                        <p>This code will expire in <strong>5 minutes</strong>.</p>

                        <p style="color: #555;">
                          If this was you, enter the code to complete your login.
                        </p>

                        <p style="color: #555;">
                          If you did not attempt to log in, you can safely ignore this email. Your account remains secure.
                        </p>

                        <p style="font-size: 12px; color: #888;">
                          For security reasons, never share this code with anyone.
                        </p>

                        <hr style="margin: 24px 0; border: none; border-top: 1px solid #eee;" />

                        <p style="font-size: 12px; color: #888; text-align: center;">
                          — HeavyRent Team
                        </p>

                      </div>

                    </div>
                    """;
        }

        try
        {
            await _emailSender.SendAsync(email, subject, body, cancellationToken );
        }
        catch (Exception ex)
        {
            throw new EmailSendFailedException(ex);
        }
    }
}