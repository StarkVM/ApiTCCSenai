namespace Listings.Application.CreateListings.Records;

public record CreateListingFreightOptionCommand(
    bool IsAvailable,
    decimal FixedPrice
    );