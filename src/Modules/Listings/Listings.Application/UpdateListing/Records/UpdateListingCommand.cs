using Listings.Domain.Enums;

namespace Listings.Application.UpdateListing.Records;

/// <summary>
/// Command used to update a listing.
/// / Comando utilizado para atualizar um anúncio.
/// </summary>
public sealed record UpdateListingCommand(
    Guid ListingId,
    Guid RequesterId,
    string Title,
    string Description,
    ListingCategory Category,
    decimal DailyPrice,
    UpdateListingPickupAddressCommand PickupAddress,
    UpdateListingOperatorOptionCommand OperatorOption,
    UpdateListingFreightOptionCommand FreightOption
);

/// <summary>
/// Pickup address data used during listing update.
/// / Dados do endereço de retirada usados na atualização do anúncio.
/// </summary>
public sealed record UpdateListingPickupAddressCommand(
    string State,
    string City,
    string District,
    string Street,
    string Number,
    string ZipCode,
    string? Complement
);

/// <summary>
/// Operator option data used during listing update.
/// / Dados da opção de operador usados na atualização do anúncio.
/// </summary>
public sealed record UpdateListingOperatorOptionCommand(
    bool IsAvailable,
    decimal AdditionalDailyPrice
);

/// <summary>
/// Freight option data used during listing update.
/// / Dados da opção de frete usados na atualização do anúncio.
/// </summary>
public sealed record UpdateListingFreightOptionCommand(
    bool IsAvailable,
    decimal FixedPrice
);