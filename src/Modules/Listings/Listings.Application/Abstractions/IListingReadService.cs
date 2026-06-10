using Listings.Application.GetListings.ReadModels;

namespace Listings.Application.Abstractions;

/// <summary>
/// Defines optimized listing read operations.
/// / Define operações otimizadas de leitura de anúncios.
/// </summary>
public interface IListingReadService
{
    Task<ListingSearchPage> SearchAsync(
        ListingSearchCriteria criteria,
        CancellationToken cancellationToken);
}