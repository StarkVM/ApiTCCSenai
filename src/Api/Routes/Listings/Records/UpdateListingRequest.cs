using Listings.Domain.Enums;

namespace Api.Routes.Listings.Records;

/// <summary>
/// Request used to update a listing.
/// / Requisição utilizada para atualizar um anúncio.
/// </summary>
public sealed record UpdateListingRequest(
    string Title,
    string Description,
    ListingCategory Category,
    decimal DailyPrice,
    UpdateListingPickupAddressRequest PickupAddress,
    UpdateListingOperatorOptionRequest OperatorOption,
    UpdateListingFreightOptionRequest FreightOption
);

public sealed record UpdateListingPickupAddressRequest(
    string State,
    string City,
    string District,
    string Street,
    string Number,
    string ZipCode,
    string? Complement
);

public sealed record UpdateListingOperatorOptionRequest(
    bool IsAvailable,
    decimal AdditionalDailyPrice
);

public sealed record UpdateListingFreightOptionRequest(
    bool IsAvailable,
    decimal FixedPrice
);