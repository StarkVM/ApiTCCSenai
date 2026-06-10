using Listings.Domain.Entities;

namespace Listings.Domain.Interfaces;

/// <summary>
/// Defines persistence operations for listings.
/// / Define as operações de persistência para anúncios.
/// </summary>
public interface IListingRepository
{
    /// <summary>
    /// Adds a new listing to the persistence context.
    /// / Adiciona um novo anúncio ao contexto de persistência.
    /// </summary>
    Task AddAsync(
        Listing listing,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets a listing by its identifier.
    /// / Busca um anúncio pelo seu identificador.
    /// </summary>
    Task<Listing?> GetByIdAsync(
        Guid listingId,
        CancellationToken cancellationToken);
    
    /// <summary>
    /// Gets the storage keys of listing images.
    /// / Obtém as chaves de armazenamento das imagens do anúncio.
    /// </summary>
    Task<IReadOnlyCollection<string>> GetImageStorageKeysAsync(
        Guid listingId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Deletes all listing images from the database.
    /// / Remove todas as imagens do anúncio do banco de dados.
    /// </summary>
    Task DeleteImagesAsync(
        Guid listingId,
        CancellationToken cancellationToken);
    
    /// <summary>
    /// Gets a listing without loading its images.
    /// / Obtém um anúncio sem carregar suas imagens.
    /// </summary>
    Task<Listing?> GetByIdForImageUpdateAsync(
        Guid listingId,
        CancellationToken cancellationToken);
    
    /// <summary>
    /// Deletes listing image rows by their storage keys.
    /// / Remove registros de imagens do anúncio pelas chaves de armazenamento.
    /// </summary>
    Task DeleteImagesByStorageKeysAsync(
        Guid listingId,
        IReadOnlyCollection<string> storageKeys,
        CancellationToken cancellationToken);
    
    Task<IReadOnlyCollection<ListingImage>> ReplaceImageRowsAndSaveAsync(
        Listing listing,
        IReadOnlyCollection<string> storageKeys,
        CancellationToken cancellationToken);
}