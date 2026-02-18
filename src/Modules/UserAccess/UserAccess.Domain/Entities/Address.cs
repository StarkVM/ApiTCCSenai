namespace UserAccess.Domain.Entities;

public class Address
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = default!;

    public string State { get; private set; } = default!;
    public string City { get; private set; } = default!;
    public string District { get; private set; } = default!;
    public string Street { get; private set; } = default!;
    public string ZipCode { get; private set; } = default!;
    
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    
    private Address() {}
    
    public Address(Guid userId, string state, string city, string district, string street, string zipCode, DateTime nowUtc)
    {
        UserId = userId;
        
        
        State = state.Trim();
        City = city.Trim();
        District = district.Trim();
        Street = street.Trim();
        ZipCode = zipCode.Trim();
        
        CreatedAt = nowUtc;
        UpdatedAt = nowUtc;
    }

    public void Update(string state, string city, string district, string street, string zipCode, DateTime nowUtc)
    {
        State = state.Trim();
        City = city.Trim();
        District = district.Trim();
        Street = street.Trim();
        ZipCode = zipCode.Trim();
        
        UpdatedAt = nowUtc;
    }
    
}