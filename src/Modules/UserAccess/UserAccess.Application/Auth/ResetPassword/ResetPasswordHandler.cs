using UserAccess.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using UserAccess.Domain.Entities;
using UserAccess.Application.Auth.ResetPassword.Records;
using UserAccess.Domain.Enums;
using UserAccess.Domain.Helpers;
using UserAccess.Domain.Results;

namespace UserAccess.Application.Auth.ResetPassword;

/// <summary>
/// Handler responsável por redefinir senha do usuário.
/// Handler responsible for resetting user password.
/// </summary>
public sealed class ResetPasswordHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IVerificationCodeService _verificationCodeService;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ResetPasswordHandler> _logger;
    public ResetPasswordHandler(
        
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IVerificationCodeService verificationCodeService,
        IClock clock,
        IUnitOfWork unitOfWork,
        ILogger<ResetPasswordHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _verificationCodeService = verificationCodeService;
        _clock = clock;
        _unitOfWork =  unitOfWork;
        _logger = logger;
    }

    public async Task<ResetPasswordResult> HandleAsync(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        // Normalização básica
        // Basic normalization
        var email = command.Email?.Trim().ToLowerInvariant();
        var newPassword = command.NewPassword?.Trim();
        var code = command.Code?.Trim();
        
        var utcNow = _clock.UtcNow;
        
        _logger.LogInformation("Starting reset password flow");
        
        Validate(email, newPassword, code);
        
        // Busca usuário
        // Find user by email
        var user = await _userRepository.GetByEmailAsync(email!, cancellationToken);

        if (user is null )
        {
            _logger.LogError("User not found");
            // Não revelar que o usuário não existe
            // Do not reveal user existence
            return new ResetPasswordResult(false);
        }

        var validation = await _verificationCodeService.ValidateAsync(
            user,
            code!,
            VerificationCodePurpose.PasswordReset,
            cancellationToken
        );
    
        if (!validation.IsValid || validation.Code is null)
        {
            return new ResetPasswordResult(false);
        }

        var verificationCode = validation.Code;

        var passwordHash = _passwordHasher.Hash(newPassword!);
        user.ChangePassword(passwordHash, utcNow);
        
        verificationCode!.Consume(utcNow);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Password reset completed successfully for email {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save password reset changes for email {Email}", email);
            throw new InvalidOperationException("DB_SAVE_FAILED", ex);
        }
        
        return new ResetPasswordResult(true);
    }

    private static void Validate(string? email, string? newPassword, string? code)
    {
        //Password validation
        if (string.IsNullOrWhiteSpace(newPassword))
        {
            throw new ArgumentException("PASSWORD_REQUIRED");
        }
        if (newPassword.Length < 8 || newPassword.Length > 50 )
        {
            throw new ArgumentException("PASSWORD_INVALID_LENGTH");
        }
        //Email validation
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("EMAIL_REQUIRED");
        }
        if (email.Length < 5 || email.Length > 255 )
        {
            throw new ArgumentException("EMAIL_INVALID_LENGTH");
        }

        if (!email.EmailIsValid())
        {
            throw new ArgumentException("EMAIL_INVALID");
        }
        //Code validation
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("CODE_IS_REQUIRED");
        }
        if (code.Length != 6)
        {
            throw new ArgumentException("CODE_INVALID_LENGTH");
        }
    }
}