using Listings.Domain.Enums;

namespace Api.Routes.Listings.Records;

/// <summary>
/// Request to create a listing using JSON.
/// / Requisição para criar um anúncio usando JSON.
/// </summary>
public sealed record CreateListingRequest(
    string Title,
    string Description,
    ListingCategory Category,
    decimal DailyPrice,
    CreateListingPickupAddressRequest PickupAddress,
    CreateListingOperatorOptionRequest OperatorOption,
    CreateListingFreightOptionRequest FreightOption,
    bool IsFleet
);

public sealed record CreateListingPickupAddressRequest(
    string State,
    string City,
    string District,
    string Street,
    string Number,
    string ZipCode,
    string? Complement
);

public sealed record CreateListingOperatorOptionRequest(
    bool IsAvailable,
    decimal AdditionalDailyPrice
);

public sealed record CreateListingFreightOptionRequest(
    bool IsAvailable,
    decimal FixedPrice
);