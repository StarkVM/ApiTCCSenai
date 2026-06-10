using Listings.Domain.Files;

namespace Listings.Application.UpdateListingImages.Records;

/// <summary>
/// Command used to replace listing images.
/// / Comando utilizado para substituir as imagens de um anúncio.
/// </summary>
public sealed record UpdateListingImagesCommand(
    Guid ListingId,
    Guid RequesterId,
    IReadOnlyCollection<ListingImageUpload> Images
);