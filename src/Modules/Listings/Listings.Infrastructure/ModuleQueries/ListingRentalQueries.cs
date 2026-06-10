using Listings.Contracts.Listings.Enums;
using Listings.Contracts.Listings.Interfaces;
using Listings.Contracts.Listings.Records;
using Listings.Domain.Enums;
using Listings.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Listings.Infrastructure.ModuleQueries;

/// <summary>
/// Implements listing queries exposed to the Rentals module.
/// / Implementa consultas de anúncios expostas ao módulo de aluguéis.
/// </summary>
public sealed class ListingRentalQueries : IListingRentalQueries
{
    private readonly ListingsDbContext _listingsDbContext;

    public ListingRentalQueries(ListingsDbContext listingsDbContext)
    {
        _listingsDbContext = listingsDbContext;
    }

    /// <summary>
    /// Gets the listing information required to create a rental.
    /// / Obtém as informações do anúncio necessárias para criar um aluguel.
    /// </summary>
    public async Task<ListingForRentalSnapshot?> GetListingForRentalAsync(
        Guid listingId,
        CancellationToken cancellationToken)
    {
        if (listingId == Guid.Empty)
        {
            return null;
        }

        var listing = await _listingsDbContext.Listings
            .AsNoTracking()
            .Where(currentListing => currentListing.Id == listingId)
            .Select(currentListing => new
            {
                currentListing.Id,
                currentListing.OwnerId,
                currentListing.Status,
                currentListing.IsFleet,
                currentListing.DailyPrice,

                OperatorAvailable =
                    currentListing.OperatorOption.IsAvailable,

                OperatorDailyPrice =
                    currentListing.OperatorOption.AdditionalDailyPrice,

                FreightAvailable =
                    currentListing.FreightOption.IsAvailable,

                FreightFixedPrice =
                    currentListing.FreightOption.FixedPrice
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (listing is null)
        {
            return null;
        }

        return new ListingForRentalSnapshot(
            listing.Id,
            listing.OwnerId,
            MapStatus(listing.Status),
            listing.IsFleet,
            listing.DailyPrice,
            listing.OperatorAvailable,
            listing.OperatorDailyPrice,
            listing.FreightAvailable,
            listing.FreightFixedPrice);
    }

    /// <summary>
    /// Maps the internal listing status to the public contract status.
    /// / Mapeia o status interno do anúncio para o status público do contrato.
    /// </summary>
    private static ListingContractStatus MapStatus(ListingStatus status)
    {
        return status switch
        {
            ListingStatus.PendingReview =>
                ListingContractStatus.PendingReview,

            ListingStatus.Approved =>
                ListingContractStatus.Approved,

            ListingStatus.Rejected =>
                ListingContractStatus.Rejected,
            
            ListingStatus.Suspended =>
                ListingContractStatus.Suspended,

            ListingStatus.Deleted =>
                ListingContractStatus.Deleted,

            
            _ => throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "Unsupported listing status.")
        };
    }
}