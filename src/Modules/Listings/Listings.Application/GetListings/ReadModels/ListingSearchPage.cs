namespace Listings.Application.GetListings.ReadModels;

public sealed record ListingSearchPage(
    IReadOnlyCollection<ListingReadModel> Items,
    int TotalCount
    );