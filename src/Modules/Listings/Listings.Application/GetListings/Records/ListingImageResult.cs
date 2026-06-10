namespace Listings.Application.GetListings.Records;

/// <summary>
/// Represents a listing image returned to the client.
/// / Representa uma imagem de anúncio retornada ao cliente.
/// </summary>
public sealed record ListingImageResult(
    Guid ImageId,
    string Url,
    int DisplayOrder,
    DateTime UrlExpiresAtUtc
);