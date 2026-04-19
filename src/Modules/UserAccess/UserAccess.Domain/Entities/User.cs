using UserAccess.Domain.Enums;
using UserAccess.Domain.Interfaces;

namespace UserAccess.Domain.Entities;

public sealed class User
{
    public Guid Id { get; private set; }

    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public DateTime BirthDate { get; private set; }

    public string Email { get; private set; } = default!;
    public string CpfHash { get; private set; } = default!;

    public string PasswordHash { get; private set; } = default!;
    
    public DateTime? PasswordChangedAt { get; private set; }

    public DateTime? EmailVerifiedAt { get; private set; }

    public DateTime CreatedAt { get; private set; }
    
    public UserStatus Status { get; private set; } = UserStatus.PendingEmailVerification;
    
    public UserType Type { get; private set; } = UserType.Renter;

    public Address? Address { get; private set; }

    private User() { }

    public User(Guid id, string firstName, string lastName, DateTime birthDate, string email, string cpfHash,
        string passwordHash, DateTime createdAt)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        BirthDate = birthDate;
        Email = email;
        CpfHash = cpfHash;
        PasswordHash = passwordHash;
        CreatedAt = createdAt;
        PasswordChangedAt = createdAt;
    }

    public void SetAddress(Address address)
    {
        Address = address;
    }

    public void RestartPendingVerification(string firstName, string lastName, DateTime birthDate, string email ,string cpfHash,
        string passwordHash, DateTime createdAt)
    {
        FirstName = firstName;
        LastName = lastName;
        BirthDate = birthDate;
        Email = email;
        CpfHash = cpfHash;
        PasswordHash = passwordHash;
        Status = UserStatus.PendingEmailVerification;
        CreatedAt = createdAt;
    }

    public void ChangePassword(string newPasswordHash, DateTime changedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
        {
            throw new ArgumentException("Password Hash cannot be empty.");
        }
        
        PasswordHash = newPasswordHash;
        PasswordChangedAt = changedAtUtc;
    }

    public void MarkAsProvider()
    {
        Type =  UserType.Provider;
    }
    
    public void MarkEmailVerified(DateTime verifiedAt)
    {
        EmailVerifiedAt = verifiedAt;
        Status = UserStatus.PendingIdentityVerification;
    }

    public void ActivateUser()
    {
        Status = UserStatus.Active;
    }
    
    public bool IsEmailVerified() => EmailVerifiedAt.HasValue;

    public void Disable() => Status = UserStatus.Disabled;
    
}