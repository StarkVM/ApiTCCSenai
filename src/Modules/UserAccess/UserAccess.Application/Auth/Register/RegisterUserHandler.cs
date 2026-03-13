using UserAccess.Domain.Interfaces;
using UserAccess.Domain.Entities;
using UserAccess.Domain.Helpers;

namespace UserAccess.Application.Auth.Register;

public sealed class RegisterUserHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICpfHasher _cpfHasher;
    private readonly IClock _clock;
    
    
    public RegisterUserHandler(IUserRepository userRepository, IPasswordHasher passwordHasher,
        ICpfHasher cpfHasher, IClock clock)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _cpfHasher = cpfHasher;
        _clock = clock;
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

        if (await _userRepository.EmailExistsAsync(email!, cancellationToken))
        {
            throw new InvalidOperationException("EMAIL_ALREADY_REGISTERED");
        }

        var cpfHash = _cpfHasher.Hash(cpf!);
        
        if (await _userRepository.CpfHashExistsAsync(cpfHash, cancellationToken))
        {
            throw new InvalidOperationException("CPF_ALREADY_REGISTERED");
        }

        var passwordHash = _passwordHasher.Hash(password!);

        var user = new User(
            Guid.NewGuid(),
            firstName!,
            lastName!,
            command.BirthDate,
            email!,
            cpfHash,
            passwordHash,
            nowUtc);
        
        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

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