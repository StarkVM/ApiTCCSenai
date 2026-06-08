using Listings.Domain.Enums;

namespace Listings.Application.CreateListings.Records;

public record CreateListingCommand(
    Guid OwnerId,
    string Title,
    string Description,
    ListingCategory Category,
    decimal DailyPrice,
    CreateListingPickupAddressCommand PickupAddress,
    CreateListingOperatorOptionCommand OperatorOption,
    CreateListingFreightOptionCommand FreightOption,
    IReadOnlyCollection<CreateListingImageCommand> Images,
    bool IsFleet
    );