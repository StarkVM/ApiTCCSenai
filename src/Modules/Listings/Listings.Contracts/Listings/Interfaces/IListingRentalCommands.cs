namespace Listings.Contracts.Listings.Interfaces;

public interface IListingRentalCommands
{
    Task<bool> TrySuspendListingForRentalAsync(
        Guid listingId,
        DateTime suspendedAtUtc,
        CancellationToken cancellationToken);
    
    Task<bool> TryReleaseListingAfterRentalAsync(
        Guid listingId,
        DateTime releasedAtUtc,
        CancellationToken cancellationToken);
}