using Listings.Contracts.Listings.Records;

namespace Listings.Contracts.Listings.Interfaces;

public interface IListingRentalQueries
{
    Task<ListingForRentalSnapshot?> GetListingForRentalAsync(
        Guid listingId,
        CancellationToken cancellationToken);
}