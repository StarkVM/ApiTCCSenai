using Microsoft.Extensions.Logging;
using UserAccess.Domain.Interfaces;
using UserAccess.Domain.Entities;
using UserAccess.Domain.Helpers;
using UserAccess.Application.Auth.Register.Records;
using UserAccess.Domain.Enums;

namespace UserAccess.Application.Auth.Register;

public sealed class RegisterUserHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICpfHasher _cpfHasher;
    private readonly IClock _clock;
    private readonly IEmailSender _emailSender;
    private readonly IVerificationCodeHasher _verificationCodeHasher;
    private readonly IEmailVerificationRepository _emailVerificationRepository;
    private readonly IUniIUnitOfWork _unitOfWork;
    private readonly ILogger<RegisterUserHandler> _logger;
    
    
    public RegisterUserHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, ICpfHasher cpfHasher, 
        IClock clock, IEmailSender emailSender, IVerificationCodeHasher verificationCodeHasher,
        IEmailVerificationRepository emailVerificationRepository,
        IUniIUnitOfWork unitOfWork, ILogger<RegisterUserHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _cpfHasher = cpfHasher;
        _clock = clock;
        _emailSender = emailSender;
        _verificationCodeHasher = verificationCodeHasher;
        _emailVerificationRepository = emailVerificationRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    
    public async Task<RegisterUserResult> HandleAsync(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var firstName = command.FirstName?.Trim();
        var lastName = command.LastName?.Trim();
        var email = command.Email?.Trim().ToLowerInvariant();
        var cpf = command.Cpf?.Clean().Trim();
        var password = command.Password?.Trim();
        
        var nowUtc = _clock.UtcNow;
        
        Validate(firstName, lastName, email, cpf, password, command.BirthDate, nowUtc);
        
        //user
        _logger.LogInformation("Starting user registration flow for email {Email}", email);
        
        var cpfHash = _cpfHasher.Hash(cpf!);
        var passwordHash = _passwordHasher.Hash(password!);
        
        var existingUser = await _userRepository.GetByEmailAsync(email!, cancellationToken);

        User user;

        if (existingUser is null)
        {
            if (await _userRepository.CpfHashExistsAsync(cpfHash, cancellationToken))
            {
                _logger.LogWarning("Registration blocked because CPF is already registered. Email: {Email}", email);
                throw new InvalidOperationException("CPF_ALREADY_REGISTERED");
            }
                user = new User(
                Guid.NewGuid(),
                firstName!,
                lastName!,
                command.BirthDate,
                email!,
                cpfHash,
                passwordHash,
                nowUtc);
                
                _logger.LogInformation("Creating new pending user registration for email {Email}", email);

        
            await _userRepository.AddAsync(user, cancellationToken);
        }
        else
        {
            if (existingUser.Status == UserStatus.Active)
            {
                _logger.LogWarning("Registration blocked because email is already active. Email: {Email}", email);
                throw new InvalidOperationException("EMAIL_ALREADY_REGISTERED");
            }
            if (await _userRepository.CpfHashExistsForAnotherUserAsync(cpfHash, existingUser.Id, cancellationToken))
            {
                _logger.LogWarning("Registration blocked because CPF is already registered. Email: {Email}", email);
                throw new InvalidOperationException("CPF_ALREADY_REGISTERED");
            }
            existingUser.RestartPendingVerification(
                firstName!,
                lastName!,
                command.BirthDate,
                cpfHash,
                passwordHash
                );
            
            _logger.LogInformation("Existing non-active user found for email {Email}. Restarting pending verification flow.", email);
            
            user = existingUser;
        }
        
        
        //email
        var code = Email.Code();
        
        var codeHash = _verificationCodeHasher.Hash(code);
        var expiresAt = nowUtc.AddMinutes(5);

        var emailVerificationCode = new EmailVerificationCode(
            Guid.NewGuid(),
                user.Id,
                codeHash,
                nowUtc,
                expiresAt
                );
        
        await _emailVerificationRepository.AddAsync( emailVerificationCode, cancellationToken);
            
        //
        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Registration data saved successfully for email {Email}", email);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to save registration data for email {Email}", email);
            throw new InvalidOperationException("DB_SAVE_FAILED", ex);
        }
        
        var body =$"""
                   Hello,

                   Your verification code is: {code}

                   This code expires in 5 minutes.

                   If you did not request this, ignore this email.
                   """;
        
        var subject = "Your Email Verification Code";

        try
        {
            await _emailSender.SendAsync(email!, subject, body, cancellationToken);
            _logger.LogInformation("Verification code sent successfully for email {Email}", email);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to send verification email for email {Email}", email);
            throw new InvalidOperationException("EMAIL_SEND_FAILED", ex);
        }
        
        return new RegisterUserResult(
            user.Id,
            user.Email,
            user.CreatedAt
        );
    }

    private static void Validate(string? firstName, string? lastName,
        string? email, string? cpf, string? password, DateTime birthDate, DateTime nowUtc)
    {
        //Names verification
        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new ArgumentException("FIRST_NAME_REQUIRED");
        }
        if (firstName.Length < 2 || firstName.Length > 100 )
        {
            throw new ArgumentException("FIRST_NAME_INVALID_LENGTH");
        }
        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new ArgumentException("LAST_NAME_REQUIRED");
        }
        if (lastName.Length < 2 || lastName.Length > 100 )
        {
            throw new ArgumentException("LAST_NAME_INVALID_LENGTH");
        }
        //Email verification
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
        //Cpf verification
        if (string.IsNullOrWhiteSpace(cpf))
        {
            throw new ArgumentException("CPF_REQUIRED");
        }
        if (cpf.Length != 11)
        {
            throw new ArgumentException("CPF_INVALID_LENGTH");
        }

        if (!cpf.CpfIsValid())
        {
            throw new ArgumentException("CPF_INVALID");
        }
        //Password verification
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("PASSWORD_REQUIRED");
        }
        if (password.Length < 8)
        {
            throw new ArgumentException("PASSWORD_TOO_SHORT");
        }
        if (password.Length > 50)
        {
            throw new ArgumentException("PASSWORD_TOO_LONG");
        }
        //BirthDate Verification
        if (birthDate == default)
        {
            throw new ArgumentException("BIRTH_DATE_REQUIRED");
        }
        if (birthDate.Date > nowUtc.Date)
        { 
            throw new ArgumentException("BIRTH_DATE_INVALID");
        }
        if (!birthDate.IsAdult(nowUtc.Date))
        {
            throw new ArgumentException("TOO_YOUNG");
        }
    }
}