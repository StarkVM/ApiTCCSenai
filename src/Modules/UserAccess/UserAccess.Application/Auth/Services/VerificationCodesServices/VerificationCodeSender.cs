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
            subject = "Verifique seu e-mail – seu código está aqui";

            body = $"""
                    <div style="font-family: Arial, sans-serif; background-color: #f5f5f5; padding: 20px;">
                      
                      <div style="max-width: 500px; margin: 0 auto; background-color: #ffffff; padding: 24px; border-radius: 8px;">
                        
                        <h2 style="margin-top: 0;">Verifique seu e-mail</h2>

                        <p>Olá,</p>

                        <p>Obrigado por se cadastrar na <strong>HeavyRent</strong>.</p>

                        <p>Seu código de verificação é:</p>

                        <div style="
                            font-size: 28px;
                            font-weight: bold;
                            letter-spacing: 6px;
                            text-align: center;
                            margin: 20px 0;
                        ">
                            {code}
                        </div>

                        <p>Este código expirará em <strong>5 minutos</strong>.</p>

                        <p style="color: #555;">
                          Caso você não tenha solicitado este código, pode ignorar este e-mail com segurança.
                        </p>

                        <p style="font-size: 12px; color: #888;">
                          Por motivos de segurança, não compartilhe este código com ninguém.
                        </p>

                        <hr style="margin: 24px 0; border: none; border-top: 1px solid #eee;" />

                        <p style="font-size: 12px; color: #888; text-align: center;">
                          — Equipe HeavyRent
                        </p>

                      </div>

                    </div>
                    """;
        }
        else if (command.Purpose == VerificationCodePurpose.PasswordReset)
        {
            subject = "Redefina sua senha – seu código está aqui";

            body = $"""
                    <div style="font-family: Arial, sans-serif; background-color: #f5f5f5; padding: 20px;">
                      
                      <div style="max-width: 500px; margin: 0 auto; background-color: #ffffff; padding: 24px; border-radius: 8px;">
                        
                        <h2 style="margin-top: 0;">Redefina sua senha</h2>

                        <p>Olá,</p>

                        <p>Recebemos uma solicitação para redefinir a senha da sua conta na <strong>HeavyRent</strong>.</p>

                        <p>Seu código de redefinição de senha é:</p>

                        <div style="
                            font-size: 28px;
                            font-weight: bold;
                            letter-spacing: 6px;
                            text-align: center;
                            margin: 20px 0;
                        ">
                            {code}
                        </div>

                        <p>Este código expirará em <strong>5 minutos</strong>.</p>

                        <p style="color: #555;">
                          Caso você não tenha solicitado a redefinição, pode ignorar este e-mail com segurança.
                        </p>

                        <p style="font-size: 12px; color: #888;">
                          Por motivos de segurança, nunca compartilhe este código com ninguém.
                        </p>

                        <hr style="margin: 24px 0; border: none; border-top: 1px solid #eee;" />

                        <p style="font-size: 12px; color: #888; text-align: center;">
                          — Equipe HeavyRent
                        </p>

                      </div>

                    </div>
                    """;
        }
        else
        {
            subject = "Verifique seu login – seu código está aqui";

            body = $"""
                    <div style="font-family: Arial, sans-serif; background-color: #f5f5f5; padding: 20px;">
                      
                      <div style="max-width: 500px; margin: 0 auto; background-color: #ffffff; padding: 24px; border-radius: 8px;">
                        
                        <h2 style="margin-top: 0;">Verifique seu login</h2>

                        <p>Olá,</p>

                        <p>Detectamos uma tentativa de login em sua conta da <strong>HeavyRent</strong>.</p>

                        <p>Use o código de verificação abaixo para continuar:</p>

                        <div style="
                            font-size: 28px;
                            font-weight: bold;
                            letter-spacing: 6px;
                            text-align: center;
                            margin: 20px 0;
                        ">
                            {code}
                        </div>

                        <p>Este código expirará em <strong>5 minutos</strong>.</p>

                        <p style="color: #555;">
                          Caso tenha sido você, informe o código para concluir o login.
                        </p>

                        <p style="color: #555;">
                          Caso você não tenha tentado entrar em sua conta, pode ignorar este e-mail com segurança. Sua conta continuará protegida.
                        </p>

                        <p style="font-size: 12px; color: #888;">
                          Por motivos de segurança, nunca compartilhe este código com ninguém.
                        </p>

                        <hr style="margin: 24px 0; border: none; border-top: 1px solid #eee;" />

                        <p style="font-size: 12px; color: #888; text-align: center;">
                          — Equipe HeavyRent
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