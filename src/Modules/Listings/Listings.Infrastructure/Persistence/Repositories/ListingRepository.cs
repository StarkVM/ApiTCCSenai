using Listings.Domain.Entities;
using Listings.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Listings.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core repository implementation for listings.
/// / Implementação de repositório EF Core para anúncios.
/// </summary>
public sealed class ListingRepository : IListingRepository
{
    private readonly ListingsDbContext _listingsDbContext;
    private readonly IClock _clock;

    public ListingRepository(ListingsDbContext listingsDbContext, IClock clock)
    {
        _listingsDbContext = listingsDbContext;
        _clock = clock;
    }

    /// <summary>
    /// Adds a new listing to the database context.
    /// / Adiciona um novo anúncio ao contexto do banco de dados.
    /// </summary>
    public async Task AddAsync(
        Listing listing,
        CancellationToken cancellationToken)
    {
        await _listingsDbContext.Listings.AddAsync(
            listing,
            cancellationToken);
    }

    /// <summary>
    /// Gets a listing by its identifier.
    /// / Busca um anúncio pelo seu identificador.
    /// </summary>
    public Task<Listing?> GetByIdAsync(
        Guid listingId,
        CancellationToken cancellationToken)
    {
        return _listingsDbContext.Listings
            .Include(listing => listing.Images)
            .FirstOrDefaultAsync(
                listing => listing.Id == listingId,
                cancellationToken);
    }
    
    public async Task DeleteImagesAsync(
        Guid listingId,
        CancellationToken cancellationToken)
    {
        await _listingsDbContext.ListingImages
            .Where(image => image.ListingId == listingId)
            .ExecuteDeleteAsync(cancellationToken);

        var trackedImages = _listingsDbContext.ChangeTracker
            .Entries<ListingImage>()
            .Where(entry => entry.Entity.ListingId == listingId)
            .ToArray();

        foreach (var trackedImage in trackedImages)
        {
            trackedImage.State = EntityState.Detached;
        }
    }
    
    public async Task<Listing?> GetByIdForImageUpdateAsync(
        Guid listingId,
        CancellationToken cancellationToken)
    {
        return await _listingsDbContext.Listings
            .SingleOrDefaultAsync(
                listing => listing.Id == listingId,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<string>> GetImageStorageKeysAsync(
        Guid listingId,
        CancellationToken cancellationToken)
    {
        return await _listingsDbContext.ListingImages
            .AsNoTracking()
            .Where(image => image.ListingId == listingId)
            .Select(image => image.StorageKey)
            .ToArrayAsync(cancellationToken);
    }

    public async Task DeleteImagesByStorageKeysAsync(
        Guid listingId,
        IReadOnlyCollection<string> storageKeys,
        CancellationToken cancellationToken)
    {
        if (storageKeys.Count == 0)
        {
            return;
        }

        await _listingsDbContext.ListingImages
            .Where(image =>
                image.ListingId == listingId &&
                storageKeys.Contains(image.StorageKey))
            .ExecuteDeleteAsync(cancellationToken);
    }
    
    public async Task<IReadOnlyCollection<ListingImage>> ReplaceImageRowsAndSaveAsync(
        Listing listing,
        IReadOnlyCollection<string> storageKeys,
        CancellationToken cancellationToken)
    {
        if (storageKeys.Count == 0)
        {
            throw new ArgumentException(
                "LISTING_MUST_HAVE_AT_LEAST_ONE_IMAGE");
        }

        await using var transaction =
            await _listingsDbContext.Database.BeginTransactionAsync(
                cancellationToken);

        await _listingsDbContext.ListingImages
            .Where(image => image.ListingId == listing.Id)
            .ExecuteDeleteAsync(cancellationToken);

        var newImages = new List<ListingImage>();

        var displayOrder = 1;

        foreach (var storageKey in storageKeys)
        {
            var image = new ListingImage(
                Guid.NewGuid(),
                listing.Id,
                storageKey,
                displayOrder,
                _clock.UtcNow
                );

            newImages.Add(image);

            displayOrder++;
        }

        await _listingsDbContext.ListingImages.AddRangeAsync(
            newImages,
            cancellationToken);

        await _listingsDbContext.SaveChangesAsync(
            cancellationToken);

        await transaction.CommitAsync(
            cancellationToken);

        return newImages;
    }
}