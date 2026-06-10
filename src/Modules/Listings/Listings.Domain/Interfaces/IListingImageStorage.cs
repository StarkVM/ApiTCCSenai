using Listings.Domain.Files;

namespace Listings.Domain.Interfaces;

/// <summary>
/// Defines storage operations for listing images.
/// / Define operações de armazenamento para imagens de anúncios.
/// </summary>
public interface IListingImageStorage
{
    Task<string> UploadAsync(
        Guid listingId,
        Guid imageId,
        string fileName,
        string contentType,
        Stream contentStream,
        CancellationToken cancellationToken);
    
    Task DeleteAsync(
        string storageKey,
        CancellationToken cancellationToken);
    
    Task<string> UpdateAsync(
        Guid listingId,
        ListingImageUpload image,
        int displayOrder,
        CancellationToken cancellationToken);
}