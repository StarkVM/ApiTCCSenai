namespace UserAccess.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }

    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public DateTime BirthDate { get; private set; }

    public string Email { get; private set; } = default!;
    public string CpfHash { get; private set; } = default!;

    public string PasswordHash { get; private set; } = default!;

    public DateTime? EmailVerifiedAt { get; private set; }

    public DateTime CreatedAt { get; private set; }

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
    }

    public void SetAddress(Address address)
    {
        Address = address;
    }
    
    public void MarkEmailVerified(DateTime verifiedAt) => EmailVerifiedAt = verifiedAt;
    
    public bool IsEmailVerified() => EmailVerifiedAt.HasValue;
    
}