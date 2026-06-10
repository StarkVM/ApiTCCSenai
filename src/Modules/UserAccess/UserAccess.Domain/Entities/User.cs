using UserAccess.Domain.Enums;
using UserAccess.Domain.Exceptions.UserAccessExceptions; 

namespace UserAccess.Domain.Entities;

public sealed class User
{
    public Guid Id { get; private set; }

    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    
    public string? ProfilePhotoStorageKey { get; private set; }
    public DateOnly BirthDate { get; private set; }

    public string Email { get; private set; } = default!;
    public string CpfHash { get; private set; } = default!;

    public string PasswordHash { get; private set; } = default!;
    
    public DateTime? PasswordChangedAt { get; private set; }

    public DateTime? EmailVerifiedAt { get; private set; }

    public DateTime CreatedAt { get; private set; }
    
    public UserStatus Status { get; private set; } = UserStatus.PendingEmailVerification;
    
    public DateTime? DisabledAt { get; private set; }
    
    public UserType Type { get; private set; } = UserType.Renter;
    
    public DateTime? BecomeProviderAt { get; private set; }
    
    public DateTime? ProfilePhotoUpdatedAtUtc { get; private set; }

    public Address? Address { get; private set; }

    private User() { }

    public User(Guid id, string firstName, string lastName, DateOnly birthDate, string email, string cpfHash,
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

    public void RestartPendingVerification(string firstName, string lastName, DateOnly birthDate, string email ,string cpfHash,
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
    public void BecomeProvider(DateTime becomeProviderAtUtc)
    {
        if (Status != UserStatus.Active)
        {
            throw new UserMustBeActiveToBecomeProviderException();
        }

        if (Type == UserType.Provider)
        {
            return;
        }

        Type = UserType.Provider;
        BecomeProviderAt = becomeProviderAtUtc;
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

    /// <summary>
    /// Marks the user as deleted.
    /// / Marca o usuário como deletado.
    /// </summary>
    public void Disable(DateTime disabledUtcAtUtc)
    {
        if (Status == UserStatus.PendingEmailVerification)
        {
            throw new UserEmailMustBeVerifiedToDeleteException();
        }

        if (Status == UserStatus.Disabled)
        {
            return;
        }

        Status = UserStatus.Disabled;
        DisabledAt = disabledUtcAtUtc;
    }
    
    public void MarkIdentityDenied() => Status = UserStatus.IdentityDenied;
    
    public string? ReplaceProfilePhoto(
        string storageKey,
        DateTime updatedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
        {
            throw new ArgumentException("PROFILE_PHOTO_STORAGE_KEY_REQUIRED");
        }

        if (updatedAtUtc == default)
        {
            throw new ArgumentException("UPDATED_AT_REQUIRED");
        }

        var oldStorageKey = ProfilePhotoStorageKey;

        ProfilePhotoStorageKey = storageKey;
        ProfilePhotoUpdatedAtUtc = updatedAtUtc;

        return oldStorageKey;
    }
    
}