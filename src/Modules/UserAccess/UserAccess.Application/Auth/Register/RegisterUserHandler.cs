using Microsoft.Extensions.Logging;
using UserAccess.Domain.Senders;
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
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegisterUserHandler> _logger;
    private readonly IAddressRepository _addressRepository;
    private readonly IVerificationCodeSender _verificationCodeSender;
    
    public RegisterUserHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, ICpfHasher cpfHasher, 
        IClock clock, IUnitOfWork unitOfWork, ILogger<RegisterUserHandler> logger,
        IAddressRepository addressRepository, IVerificationCodeSender  verificationCodeSender)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _cpfHasher = cpfHasher;
        _clock = clock;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _addressRepository = addressRepository;
        _verificationCodeSender = verificationCodeSender;
    }
    
    public async Task<RegisterUserResult> HandleAsync(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var firstName = command.FirstName?.Trim();
        var lastName = command.LastName?.Trim();
        var email = command.Email?.Trim().ToLowerInvariant();
        var cpf = command.Cpf?.Clean().Trim();
        var password = command.Password?.Trim();
        var birthDate = command.BirthDate;
        
        var nowUtc = _clock.UtcNow;
        
        var address = new Address(command.Address.State, command.Address.City, command.Address.District,
            command.Address.Street,command.Address.ZipCode, nowUtc);
        
        var today = DateOnly.FromDateTime(nowUtc);
        
        Validate(firstName, lastName, email, cpf, password, birthDate,address ,today);
        
        //user
        _logger.LogInformation(
            "Starting user registration flow for email {Email}.", email);
        
        var cpfHash = _cpfHasher.Hash(cpf!);
        var passwordHash = _passwordHasher.Hash(password!);
        
        var existingUserByEmail = await _userRepository.GetByEmailAsync(email!, cancellationToken);
        var existingUserByCpf = await _userRepository.GetByCpfAsync(cpfHash, cancellationToken);
        
        var existingUser =  existingUserByEmail ?? existingUserByCpf;

        User user;
        
        if (existingUser is not null)
        {
            if (existingUserByEmail is not null && existingUserByCpf is not null &&
                existingUserByEmail.Id != existingUserByCpf.Id)
            {
                _logger.LogWarning(
                    "Registration blocked because email and CPF belong to different users. Email: {Email}. EmailUserId: {EmailUserId}. CpfUserId: {CpfUserId}",
                    email,
                    existingUserByEmail?.Id,
                    existingUserByCpf?.Id);

                throw new InvalidOperationException("EMAIL_AND_CPF_CONFLICT");
            }
            if (existingUser.Status != UserStatus.PendingEmailVerification)
            {
                _logger.LogWarning("Registration blocked because email or cpf already used. Email: {Email}", existingUser.Email);
                throw new InvalidOperationException("EMAIL_OR_CPF_CONFLICT");
            }
            /*if (await _addressRepository.AddressIsValid(address, cancellationToken))
            {
                _logger.LogWarning("Registration blocked because Address is not Valid. Email: {Email}", email);
                throw new InvalidOperationException("ADDRESS_NOT_VALID");
            }*/

            if (existingUser.CreatedAt.AddMinutes(5) > nowUtc)
            {
                _logger.LogWarning("Registration blocked because a registration already in progress. Email: {Email}", existingUser.Email);
                throw new InvalidOperationException("REGISTRATION_IN_PROGRESS");
            }
            
            existingUser.RestartPendingVerification(
                firstName!,
                lastName!,
                birthDate,
                email!,
                cpfHash,
                passwordHash,
                nowUtc
                );
            
            var existingAddress = await  _addressRepository.GetAddressByUserIdAsync(existingUser.Id, cancellationToken);
            
            if (existingAddress is null)
            {
                _logger.LogWarning("Registration blocked because Addrress was not found. UserId {UserId}", existingUser.Id );
                throw new InvalidOperationException("ADDRESS_NOT_FOUND");
            }
            
            existingAddress!.Update(address.State,address.City,address.District,address.Street,address.ZipCode, nowUtc);
            existingUser.SetAddress(existingAddress);
            
            _logger.LogInformation("Existing non-active user found for email {Email}. Restarting pending verification flow.", existingUser.Email);
            
            
            user = existingUser;
        }
        else
        {
            /*if (await _addressRepository.AddressIsValid(address, cancellationToken))
            {
                _logger.LogWarning("Registration blocked because Address is not Valid. Email: {Email}", email);
                throw new InvalidOperationException("ADDRESS_NOT_VALID");
            }*/
                
            user = new User(
                Guid.NewGuid(),
                firstName!,
                lastName!,
                birthDate,
                email!,
                cpfHash,
                passwordHash,
                nowUtc);
                
            address.SetUserId(user.Id);
            user.SetAddress(address);
                
            await _addressRepository.AddAddressAsync(address, cancellationToken);
                
            _logger.LogInformation("Creating new pending user registration for email {Email}", email);
            
            await _userRepository.AddAsync(user, cancellationToken);
        }
        
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

        //email
        
        var emailSenderCommand = new SendVerificationCodeRequest(
            email!,
            user.Id,
            VerificationCodePurpose.EmailVerification
        );
        
        try
        {
            await _verificationCodeSender.SendCodeAsync(emailSenderCommand, cancellationToken);
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
        string? email, string? cpf, string? password, DateOnly birthDate, Address address, DateOnly today)
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
        if (birthDate > today)
        { 
            throw new ArgumentException("BIRTH_DATE_INVALID");
        }
        if (!birthDate.IsAdult(today))
        {
            throw new ArgumentException("TOO_YOUNG");
        }
        //Address
        if (string.IsNullOrWhiteSpace(address.State))
        {
            throw new ArgumentException("ADDRESS_STATE_REQUIRED");
        }

        if (string.IsNullOrWhiteSpace(address.City))
        {
            throw new ArgumentException("ADDRESS_CITY_REQUIRED");
        }

        if (string.IsNullOrWhiteSpace(address.District))
        {
            throw new ArgumentException("ADDRESS_DISTRICT_REQUIRED");
        }

        if (string.IsNullOrWhiteSpace(address.Street))
        {
            throw new ArgumentException("ADDRESS_STREET_REQUIRED");
        }

        if (string.IsNullOrWhiteSpace(address.ZipCode))
        {
            throw new ArgumentException("ADDRESS_ZIPCODE_REQUIRED");
        }
    }
}