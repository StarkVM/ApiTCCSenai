namespace Listings.Application.CreateListings.Records;

public record CreateListingPickupAddressCommand(
    string State,
    string City,
    string District,
    string Street,
    string Number,
    string ZipCode,
    string? Complement
    );