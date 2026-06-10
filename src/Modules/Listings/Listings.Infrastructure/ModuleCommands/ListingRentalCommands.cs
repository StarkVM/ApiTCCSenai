using Listings.Contracts.Listings.Interfaces;
using Listings.Domain.Enums;
using Listings.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Listings.Infrastructure.ModuleCommands;

/// <summary>
/// Implements listing commands exposed to the Rentals module.
/// / Implementa comandos de anúncios expostos ao módulo de aluguéis.
/// </summary>
public sealed class ListingRentalCommands : IListingRentalCommands
{
    private readonly ListingsDbContext _listingsDbContext;
    private readonly ILogger<ListingRentalCommands> _logger;

    public ListingRentalCommands(
        ListingsDbContext listingsDbContext,
        ILogger<ListingRentalCommands> logger)
    {
        _listingsDbContext = listingsDbContext;
        _logger = logger;
    }

    /// <summary>
    /// Tries to suspend a non-fleet listing after rental creation.
    /// / Tenta suspender um anúncio que não representa frota após a criação do aluguel.
    /// </summary>
    public async Task<bool> TrySuspendListingForRentalAsync(
        Guid listingId,
        DateTime suspendedAtUtc,
        CancellationToken cancellationToken)
    {
        if (listingId == Guid.Empty)
        {
            return false;
        }

        var listing = await _listingsDbContext.Listings
            .SingleOrDefaultAsync(
                currentListing => currentListing.Id == listingId,
                cancellationToken);

        if (listing is null)
        {
            _logger.LogWarning(
                "Listing suspension failed because listing was not found. ListingId: {ListingId}",
                listingId);

            return false;
        }

        if (listing.IsFleet)
        {
            _logger.LogWarning(
                "Listing suspension was rejected because listing represents a fleet. ListingId: {ListingId}",
                listingId);

            return false;
        }

        if (listing.Status != ListingStatus.Approved)
        {
            _logger.LogWarning(
                "Listing suspension failed because listing is not approved. ListingId: {ListingId}, Status: {Status}",
                listingId,
                listing.Status);

            return false;
        }

        listing.SuspendForRental(suspendedAtUtc);

        await _listingsDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Listing suspended successfully after rental creation. ListingId: {ListingId}, SuspendedAtUtc: {SuspendedAtUtc}",
            listingId,
            suspendedAtUtc);

        return true;
    }
    

    public async Task<bool> TryReleaseListingAfterRentalAsync(
        Guid listingId,
        DateTime releasedAtUtc,
        CancellationToken cancellationToken)
    {
        if (listingId == Guid.Empty)
        {
            return false;
        }

        var listing = await _listingsDbContext.Listings
            .SingleOrDefaultAsync(
                currentListing => currentListing.Id == listingId,
                cancellationToken);

        if (listing is null)
        {
            _logger.LogWarning(
                "Listing release failed because listing was not found. ListingId: {ListingId}",
                listingId);

            return false;
        }
        
        if (listing.IsFleet)
        {
            _logger.LogInformation(
                "Listing release skipped because listing represents a fleet. ListingId: {ListingId}",
                listingId);

            return true;
        }
        
        if (listing.Status == ListingStatus.Approved)
        {
            return true;
        }
        
        if (listing.Status == ListingStatus.Deleted)
        {
            _logger.LogInformation(
                "Listing release skipped because listing is disabled. ListingId: {ListingId}",
                listingId);

            return true;
        }

        if (listing.Status != ListingStatus.Suspended)
        {
            _logger.LogWarning(
                "Listing release failed because listing has an invalid status. ListingId: {ListingId}, Status: {Status}",
                listingId,
                listing.Status);

            return false;
        }

        listing.ReleaseAfterRental(releasedAtUtc);

        await _listingsDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Listing released successfully after rental completion. ListingId: {ListingId}, ReleasedAtUtc: {ReleasedAtUtc}",
            listingId,
            releasedAtUtc);

        return true;
    }
}