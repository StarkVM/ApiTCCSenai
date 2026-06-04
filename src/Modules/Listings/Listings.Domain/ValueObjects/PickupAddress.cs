namespace Listings.Domain.ValueObjects;

/// <summary>
/// Represents the full pickup address of a listed machine.
/// / Representa o endereço completo de retirada de uma máquina anunciada.
/// </summary>
public sealed class PickupAddress
{
    
    public string State { get; private set; } = default!;

   
    public string City { get; private set; } = default!;

    
    public string District { get; private set; } = default!;

    
    public string Street { get; private set; } = default!;

   
    public string Number { get; private set; } = default!;

    public string ZipCode { get; private set; } = default!;

    
    public string? Complement { get; private set; }

    private PickupAddress()
    {
    }

    public PickupAddress(
        string state,
        string city,
        string district,
        string street,
        string number,
        string zipCode,
        string? complement)
    {
        if (string.IsNullOrWhiteSpace(state))
        {
            throw new ArgumentException("State cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(city))
        {
            throw new ArgumentException("City cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(district))
        {
            throw new ArgumentException("District cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(street))
        {
            throw new ArgumentException("Street cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(number))
        {
            throw new ArgumentException("Number cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(zipCode))
        {
            throw new ArgumentException("Zip code cannot be empty.");
        }

        State = state.Trim();
        City = city.Trim();
        District = district.Trim();
        Street = street.Trim();
        Number = number.Trim();
        ZipCode = new string(zipCode.Where(char.IsDigit).ToArray());
        Complement = string.IsNullOrWhiteSpace(complement)
            ? null
            : complement.Trim();

        if (ZipCode.Length != 8)
        {
            throw new ArgumentException("Zip code must contain 8 digits.");
        }
    }
}
