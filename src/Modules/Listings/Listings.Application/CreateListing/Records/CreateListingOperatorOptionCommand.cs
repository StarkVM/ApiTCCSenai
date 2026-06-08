namespace Listings.Application.CreateListings.Records;

public record CreateListingOperatorOptionCommand(
    bool IsAvailable,
    decimal AdditionalDailyPrice
    );