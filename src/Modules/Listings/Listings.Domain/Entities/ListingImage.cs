namespace Listings.Domain.Entities;

public class ListingImage
{
    /// <summary>
    /// Unique identifier of the image record.
    /// / Identificador único do registro da imagem.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Identifier of the listing that owns the image.
    /// / Identificador do anúncio ao qual a imagem pertence.
    /// </summary>
    public Guid ListingId { get; private set; }

    /// <summary>
    /// Navigation property to the listing.
    /// / Propriedade de navegação para o anúncio.
    /// </summary>
    public Listing Listing { get; private set; } = default!;

    /// <summary>
    /// Object key/path used to locate the image in S3.
    /// / Chave/caminho usado para localizar a imagem no S3.
    /// </summary>
    public string StorageKey { get; private set; } = default!;

    /// <summary>
    /// Display order of the image inside the listing gallery.
    /// / Ordem de exibição da imagem dentro da galeria do anúncio.
    /// </summary>
    public int DisplayOrder { get; private set; }

    /// <summary>
    /// UTC date when the image record was created.
    /// / Data UTC em que o registro da imagem foi criado.
    /// </summary>
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
        if (listingId == Guid.Empty)
        {
            throw new ArgumentException("Listing id cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(storageKey))
        {
            throw new ArgumentException("Storage key cannot be empty.");
        }

        Id = id;
        ListingId = listingId;
        StorageKey = storageKey.Trim();
        DisplayOrder = displayOrder;
        CreatedAtUtc = createdAtUtc;
    }
}