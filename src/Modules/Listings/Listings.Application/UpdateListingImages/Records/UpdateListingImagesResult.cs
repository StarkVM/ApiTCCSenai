using Listings.Domain.Enums;

namespace Listings.Application.UpdateListingImages.Records;

/// <summary>
/// Result returned after replacing listing images.
/// / Resultado retornado após substituir as imagens de um anúncio.
/// </summary>
public sealed record UpdateListingImagesResult(
    Guid ListingId,
    ListingStatus Status,
    DateTime UpdatedAtUtc,
    IReadOnlyCollection<UpdatedListingImageResult> Images
);

/// <summary>
/// Represents an updated listing image returned to the client.
/// / Representa uma imagem atualizada do anúncio retornada ao cliente.
/// </summary>
public sealed record UpdatedListingImageResult(
    Guid ImageId,
    string Url,
    int DisplayOrder,
    DateTime UrlExpiresAtUtc
);