using Listings.Application.Abstractions;
using Listings.Application.GetListings.ReadModels;
using Listings.Domain.Enums;
using Listings.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Listings.Infrastructure.Queries;

/// <summary>
/// EF Core implementation of optimized listing read operations.
/// / Implementação EF Core das operações otimizadas de leitura de anúncios.
/// </summary>
public sealed class ListingReadService : IListingReadService
{
    private readonly ListingsDbContext _listingsDbContext;

    public ListingReadService(ListingsDbContext listingsDbContext)
    {
        _listingsDbContext = listingsDbContext;
    }

    public async Task<ListingSearchPage> SearchAsync(
        ListingSearchCriteria criteria,
        CancellationToken cancellationToken)
    {
        var query = _listingsDbContext.Listings
            .AsNoTracking()
            .AsQueryable();

        if (criteria.PublicOnly)
        {
            query = query.Where(
                listing => listing.Status == ListingStatus.Approved);
        }

        if (criteria.OwnerId.HasValue)
        {
            query = query.Where(
                listing => listing.OwnerId == criteria.OwnerId.Value);
        }

        if (!criteria.PublicOnly &&
            criteria.Status.HasValue)
        {
            query = query.Where(
                listing => listing.Status == criteria.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(criteria.Name))
        {
            var pattern = $"%{criteria.Name}%";

            query = query.Where(
                listing => EF.Functions.ILike(
                    listing.Title,
                    pattern));
        }

        if (criteria.Category.HasValue)
        {
            query = query.Where(
                listing => listing.Category == criteria.Category.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var rows = await query
            .OrderByDescending(listing => listing.CreatedAtUtc)
            .Skip(criteria.Skip)
            .Take(criteria.Take)
            .Select(listing => new ListingRow(
                listing.Id,
                listing.OwnerId,
                listing.Title,
                listing.Description,
                listing.Category,
                listing.DailyPrice,
                listing.IsFleet,
                listing.Status,
                listing.OperatorOption.IsAvailable,
                listing.OperatorOption.AdditionalDailyPrice,
                listing.FreightOption.IsAvailable,
                listing.FreightOption.FixedPrice,
                listing.PickupAddress.State,
                listing.PickupAddress.City,
                listing.PickupAddress.District,
                listing.PickupAddress.Street,
                listing.PickupAddress.Number,
                listing.PickupAddress.ZipCode,
                listing.PickupAddress.Complement,
                listing.RejectionReason,
                listing.CreatedAtUtc,
                listing.UpdatedAtUtc))
            .ToListAsync(cancellationToken);

        if (rows.Count == 0)
        {
            return new ListingSearchPage(
                Array.Empty<ListingReadModel>(),
                totalCount);
        }

        var listingIds = rows
            .Select(row => row.ListingId)
            .ToArray();

        var imageRows = await _listingsDbContext.ListingImages
            .AsNoTracking()
            .Where(image => listingIds.Contains(image.ListingId))
            .OrderBy(image => image.ListingId)
            .ThenBy(image => image.DisplayOrder)
            .Select(image => new ListingImageRow(
                image.Id,
                image.ListingId,
                image.StorageKey,
                image.DisplayOrder))
            .ToListAsync(cancellationToken);

        var imagesByListingId = imageRows
            .ToLookup(image => image.ListingId);

        var items = rows
            .Select(row => new ListingReadModel(
                row.ListingId,
                row.OwnerId,
                row.Title,
                row.Description,
                row.Category,
                row.DailyPrice,
                row.IsFleet,
                row.Status,
                row.OperatorAvailable,
                row.OperatorDailyPrice,
                row.FreightAvailable,
                row.FreightFixedPrice,
                row.PickupState,
                row.PickupCity,
                row.PickupDistrict,
                row.PickupStreet,
                row.PickupNumber,
                row.PickupZipCode,
                row.PickupComplement,
                row.RejectionReason,
                row.CreatedAtUtc,
                row.UpdatedAtUtc,
                imagesByListingId[row.ListingId]
                    .Select(image => new ListingImageReadModel(
                        image.ImageId,
                        image.StorageKey,
                        image.DisplayOrder))
                    .ToArray()))
            .ToArray();

        return new ListingSearchPage(
            items,
            totalCount);
    }

    private sealed record ListingRow(
        Guid ListingId,
        Guid OwnerId,
        string Title,
        string Description,
        ListingCategory Category,
        decimal DailyPrice,
        bool IsFleet,
        ListingStatus Status,
        bool OperatorAvailable,
        decimal OperatorDailyPrice,
        bool FreightAvailable,
        decimal FreightFixedPrice,
        string PickupState,
        string PickupCity,
        string PickupDistrict,
        string PickupStreet,
        string PickupNumber,
        string PickupZipCode,
        string? PickupComplement,
        string? RejectionReason,
        DateTime CreatedAtUtc,
        DateTime UpdatedAtUtc
    );

    private sealed record ListingImageRow(
        Guid ImageId,
        Guid ListingId,
        string StorageKey,
        int DisplayOrder
    );
    
    public async Task<ListingReadModel?> GetPublicByIdAsync(
        Guid listingId,
        CancellationToken cancellationToken)
    {
        if (listingId == Guid.Empty)
        {
            return null;
        }

        var listing = await _listingsDbContext.Listings
            .AsNoTracking()
            .Where(currentListing =>
                currentListing.Id == listingId &&
                currentListing.Status == ListingStatus.Approved)
            .Select(currentListing => new ListingRow(
                currentListing.Id,
                currentListing.OwnerId,
                currentListing.Title,
                currentListing.Description,
                currentListing.Category,
                currentListing.DailyPrice,
                currentListing.IsFleet,
                currentListing.Status,
                currentListing.OperatorOption.IsAvailable,
                currentListing.OperatorOption.AdditionalDailyPrice,
                currentListing.FreightOption.IsAvailable,
                currentListing.FreightOption.FixedPrice,
                currentListing.PickupAddress.State,
                currentListing.PickupAddress.City,
                currentListing.PickupAddress.District,
                currentListing.PickupAddress.Street,
                currentListing.PickupAddress.Number,
                currentListing.PickupAddress.ZipCode,
                currentListing.PickupAddress.Complement,
                currentListing.RejectionReason,
                currentListing.CreatedAtUtc,
                currentListing.UpdatedAtUtc))
            .SingleOrDefaultAsync(cancellationToken);

        if (listing is null)
        {
            return null;
        }

        var images = await _listingsDbContext.ListingImages
            .AsNoTracking()
            .Where(image => image.ListingId == listingId)
            .OrderBy(image => image.DisplayOrder)
            .Select(image => new ListingImageReadModel(
                image.Id,
                image.StorageKey,
                image.DisplayOrder))
            .ToArrayAsync(cancellationToken);

        return new ListingReadModel(
            listing.ListingId,
            listing.OwnerId,
            listing.Title,
            listing.Description,
            listing.Category,
            listing.DailyPrice,
            listing.IsFleet,
            listing.Status,
            listing.OperatorAvailable,
            listing.OperatorDailyPrice,
            listing.FreightAvailable,
            listing.FreightFixedPrice,
            listing.PickupState,
            listing.PickupCity,
            listing.PickupDistrict,
            listing.PickupStreet,
            listing.PickupNumber,
            listing.PickupZipCode,
            listing.PickupComplement,
            listing.RejectionReason,
            listing.CreatedAtUtc,
            listing.UpdatedAtUtc,
            images);
    }
}