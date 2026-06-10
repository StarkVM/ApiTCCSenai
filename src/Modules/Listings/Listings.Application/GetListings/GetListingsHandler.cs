using Listings.Application.Abstractions;
using Listings.Application.GetListings.ReadModels;
using Listings.Application.GetListings.Records;
using Listings.Domain.Enums;
using Microsoft.Extensions.Logging;
using UserAccess.Contracts.Users.Interfaces;

namespace Listings.Application.GetListings;

/// <summary>
/// Handles listing searches.
/// / Manipula pesquisas de anúncios.
/// </summary>
public sealed class GetListingsHandler
{
    private const int MaximumPageSize = 50;

    private readonly IListingReadService _listingReadService;
    private readonly IListingImageUrlProvider _imageUrlProvider;
    private readonly ILogger<GetListingsHandler> _logger;
    private readonly IUserPublicProfileQueries _userPublicProfileQueries;

    public GetListingsHandler(
        IListingReadService listingReadService,
        IListingImageUrlProvider imageUrlProvider,
        ILogger<GetListingsHandler> logger,
        IUserPublicProfileQueries userPublicProfileQueries)
    {
        _listingReadService = listingReadService;
        _imageUrlProvider = imageUrlProvider;
        _logger = logger;
        _userPublicProfileQueries = userPublicProfileQueries;
    }

    /// <summary>
    /// Searches public listings or the authenticated user's listings.
    /// / Pesquisa anúncios públicos ou anúncios do usuário autenticado.
    /// </summary>
    public async Task<GetListingsResult> HandleAsync(
        GetListingsQuery query,
        CancellationToken cancellationToken)
    {
        ValidateQuery(query);

        string? normalizedName;

        if (string.IsNullOrWhiteSpace(query.Name))
        {
            normalizedName = null;
        }
        else
        {
            normalizedName = query.Name.Trim();
        }

        Guid? ownerId;
        ListingStatus? status;

        if (query.Mine)
        {
            ownerId = query.RequesterId;
            status = query.Status;
        }
        else
        {
            ownerId = null;
            status = null;
        }

        var criteria = new ListingSearchCriteria(
            ownerId,
            PublicOnly: !query.Mine,
            normalizedName,
            query.Category,
            status,
            Skip: (query.Page - 1) * query.PageSize,
            Take: query.PageSize);

        var searchPage = await _listingReadService.SearchAsync(
            criteria,
            cancellationToken);
        
        var ownerIds = searchPage.Items
            .Select(listing => listing.OwnerId)
            .Distinct()
            .ToArray();

        var providerProfiles =
            await _userPublicProfileQueries.GetByIdsAsync(
                ownerIds,
                cancellationToken);

        var providersById = providerProfiles
            .ToDictionary(
                profile => profile.UserId,
                profile => profile.FullName);

        var items = searchPage.Items
            .Select(listing =>
            {
                providersById.TryGetValue(
                    listing.OwnerId,
                    out var providerName);

                return MapListing(
                    listing,
                    providerName);
            })
            .ToArray();

        int totalPages;

        if (searchPage.TotalCount == 0)
        {
            totalPages = 0;
        }
        else
        {
            totalPages = (int)Math.Ceiling(
                searchPage.TotalCount / (double)query.PageSize);
        }

        _logger.LogInformation(
            "Listings search completed. Mine: {Mine}, RequesterId: {RequesterId}, Name: {Name}, Category: {Category}, Status: {Status}, Page: {Page}, PageSize: {PageSize}, TotalCount: {TotalCount}",
            query.Mine,
            query.RequesterId,
            normalizedName,
            query.Category,
            status,
            query.Page,
            query.PageSize,
            searchPage.TotalCount);

        return new GetListingsResult(
            items,
            query.Page,
            query.PageSize,
            searchPage.TotalCount,
            totalPages);
    }

    private ListingResult MapListing(ListingReadModel listing, string? providerName)
    {
        var images = listing.Images
            .OrderBy(image => image.DisplayOrder)
            .Select(image =>
            {
                var accessUrl = _imageUrlProvider.Generate(
                    image.StorageKey);

                return new ListingImageResult(
                    image.ImageId,
                    accessUrl.Url,
                    image.DisplayOrder,
                    accessUrl.ExpiresAtUtc);
            })
            .ToArray();

        return new ListingResult(
            listing.ListingId,
            listing.OwnerId,
            providerName?.ToUpperInvariant(),
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

    private static void ValidateQuery(GetListingsQuery query)
    {
        if (query.Mine && query.RequesterId is null)
        {
            throw new UnauthorizedAccessException(
                "AUTHENTICATED_USER_REQUIRED");
        }

        if (!query.Mine && query.Status is not null)
        {
            throw new ArgumentException(
                "STATUS_FILTER_IS_ONLY_AVAILABLE_FOR_OWN_LISTINGS");
        }

        if (query.Page < 1)
        {
            throw new ArgumentException(
                "PAGE_MUST_BE_GREATER_THAN_ZERO");
        }

        if (query.PageSize < 1 ||
            query.PageSize > MaximumPageSize)
        {
            throw new ArgumentException(
                "PAGE_SIZE_MUST_BE_BETWEEN_1_AND_50");
        }
    }
}