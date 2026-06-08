namespace Listings.Domain.Entities;

/// <summary>
/// Represents an image stored in S3 and linked to a listing.
/// / Representa uma imagem armazenada no S3 e vinculada a um anúncio.
/// </summary>
public sealed class ListingImage
{
    public Guid Id { get; private set; }
    public Guid ListingId { get; private set; }
    public Listing Listing { get; private set; } = default!;
    public string StorageKey { get; private set; } = default!;
    public int DisplayOrder { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private ListingImage()
    {
    }

    internal ListingImage(
        Guid id,
        Guid listingId,
        string storageKey,
        int displayOrder,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Image id cannot be empty.");
        }

        if (listingId == Guid.Empty)
        {
            throw new ArgumentException("Listing id cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(storageKey))
        {
            throw new ArgumentException("Storage key cannot be empty.");
        }

        if (displayOrder is < 1 or > 5)
        {
            throw new ArgumentException("Display order must be between 1 and 5.");
        }

        Id = id;
        ListingId = listingId;
        StorageKey = storageKey.Trim();
        DisplayOrder = displayOrder;
        CreatedAtUtc = createdAtUtc;
    }
}