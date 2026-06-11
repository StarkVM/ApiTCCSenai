using Listings.Application.Abstractions;
using Listings.Application.GetListingById.Records;
using Listings.Application.GetListings.ReadModels;
using Listings.Application.GetListings.Records;
using Listings.Domain.Exceptions.ListingsExceptions;
using Microsoft.Extensions.Logging;
using UserAccess.Contracts.Users.Interfaces;
using UserAccess.Contracts.Users.Records;

namespace Listings.Application.GetListingById;

/// <summary>
/// Handles the public listing details query.
/// / Manipula a consulta pública dos detalhes de um anúncio.
/// </summary>
public sealed class GetListingByIdHandler
{
    private readonly IListingReadService _listingReadService;
    private readonly IListingImageUrlProvider _imageUrlProvider;
    private readonly IUserPublicProfileQueries _userPublicProfileQueries;
    private readonly ILogger<GetListingByIdHandler> _logger;

    public GetListingByIdHandler(
        IListingReadService listingReadService,
        IListingImageUrlProvider imageUrlProvider,
        IUserPublicProfileQueries userPublicProfileQueries,
        ILogger<GetListingByIdHandler> logger)
    {
        _listingReadService = listingReadService;
        _imageUrlProvider = imageUrlProvider;
        _userPublicProfileQueries = userPublicProfileQueries;
        _logger = logger;
    }

    /// <summary>
    /// Gets the details of a publicly available listing.
    /// / Obtém os detalhes de um anúncio publicamente disponível.
    /// </summary>
    public async Task<GetListingByIdResult> HandleAsync(
        GetListingByIdQuery query,
        CancellationToken cancellationToken)
    {
        if (query.ListingId == Guid.Empty)
        {
            throw new ArgumentException("LISTING_ID_REQUIRED");
        }

        var listing = await _listingReadService.GetPublicByIdAsync(
            query.ListingId,
            cancellationToken);

        if (listing is null)
        {
            throw new ListingNotFoundException();
        }

        var providerProfile =
            await _userPublicProfileQueries.GetByIdWithPhotoAsync(
                listing.OwnerId,
                cancellationToken);

        return MapListing(
            listing,
            providerProfile);
    }

    private GetListingByIdResult MapListing(
        ListingReadModel listing,
        UserPublicProfileWithPhotoSnapshot? providerProfile)
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

        return new GetListingByIdResult(
            listing.ListingId,
            listing.OwnerId,
            providerProfile?.FullName,
            providerProfile?.ProfilePhotoUrl,
            providerProfile?.ProfilePhotoUrlExpiresAtUtc,
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