using Microsoft.Extensions.Logging;
using UserAccess.Domain.Senders;
using UserAccess.Domain.Interfaces;
using UserAccess.Domain.Entities;
using UserAccess.Domain.Helpers;
using UserAccess.Application.Auth.Register.Records;
using UserAccess.Application.Common.Exceptions;
using UserAccess.Domain.Enums;
using UserAccess.Domain.Exceptions.UserAccessExceptions;

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
    private readonly ICpfValidator _cpfValidator;
    
    public RegisterUserHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, ICpfHasher cpfHasher, 
        IClock clock, IUnitOfWork unitOfWork, ILogger<RegisterUserHandler> logger,
        IAddressRepository addressRepository, IVerificationCodeSender  verificationCodeSender, ICpfValidator cpfValidator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _cpfHasher = cpfHasher;
        _clock = clock;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _addressRepository = addressRepository;
        _verificationCodeSender = verificationCodeSender;
        _cpfValidator = cpfValidator;
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

                throw new EmailAndCpfConflictException();
            }
            if (existingUser.Status != UserStatus.PendingEmailVerification)
            {
                _logger.LogWarning("Registration blocked because email or cpf already used. Email: {Email}", existingUser.Email);
                throw new EmailOrCpfConflictException();
            }
            /*if (await _addressRepository.AddressIsValid(address, cancellationToken))
            {
                _logger.LogWarning("Registration blocked because Address is not Valid. Email: {Email}", email);
                throw new InvalidOperationException("ADDRESS_NOT_VALID");
            }*/

            if (existingUser.CreatedAt.AddMinutes(5) > nowUtc)
            {
                _logger.LogWarning("Registration blocked because a registration already in progress. Email: {Email}", existingUser.Email);
                throw new RegistrationInProgressException();
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
                throw new AddressNotFoundException();
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
        
        var fullName = $"{firstName} {lastName}";

        var validData = await _cpfValidator.ValidateAsync(
            cpf!,
            fullName,
            birthDate,
            cancellationToken
        );

        if (!validData)
        {
            _logger.LogWarning("Registration blocked because cpf validation failed.");
            throw new CpfValidationFailedException();
        }
        
        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Registration data saved successfully for email {Email}", email);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to save registration data for email {Email}", email);
            throw new DatabaseSaveFailedException(ex);
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
            throw new EmailSendFailedException(ex);
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
            throw new ArgumentException("First name is required.");
        }
        if (firstName.Length < 2 || firstName.Length > 100 )
        {
            throw new ArgumentException("First name must be between 2 and 100 characters.");
        }
        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new ArgumentException("Last name is required.");
        }
        if (lastName.Length < 2 || lastName.Length > 100 )
        {
            throw new ArgumentException("Last name must be between 2 and 100 characters.");
        }
        //Email verification
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.");
        }
        if (email.Length < 5 || email.Length > 255 )
        {
            throw new ArgumentException("Email must be between 5 and 255 characters.");
        }

        if (!email.EmailIsValid())
        {
            throw new ArgumentException("Invalid email format.");
        }
        //Cpf verification
        if (string.IsNullOrWhiteSpace(cpf))
        {
            throw new ArgumentException("CPF is required.");
        }
        if (cpf.Length != 11)
        {
            throw new ArgumentException("CPF must contain exactly 11 digits.");
        }

        if (!cpf.CpfIsValid())
        {
            throw new ArgumentException("Invalid CPF.");
        }
        //Password verification
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password is required.");
        }
        if (password.Length < 8)
        {
            throw new ArgumentException("Password must be at least 8 characters long.");
        }
        if (password.Length > 50)
        {
            throw new ArgumentException("Password must be at most 50 characters long.");
        }
        //BirthDate Verification
        if (birthDate == default)
        {
            throw new ArgumentException("Birth date is required.");
        }
        if (birthDate > today)
        { 
            throw new ArgumentException("Birth date cannot be in the future.");
        }
        if (!birthDate.IsAdult(today))
        {
            throw new ArgumentException("User must be at least 18 years old.");
        }
        //Address
        if (address is null)
        {
            throw new ArgumentException("Address is required.");
        }
        if (string.IsNullOrWhiteSpace(address.State))
        {
            throw new ArgumentException("Address state is required.");
        }

        if (string.IsNullOrWhiteSpace(address.City))
        {
            throw new ArgumentException("Address city is required.");
        }

        if (string.IsNullOrWhiteSpace(address.District))
        {
            throw new ArgumentException("Address district is required.");
        }

        if (string.IsNullOrWhiteSpace(address.Street))
        {
            throw new ArgumentException("Address street is required.");
        }

        if (string.IsNullOrWhiteSpace(address.ZipCode))
        {
            throw new ArgumentException("Address ZIP code is required.");
        }
    }
}