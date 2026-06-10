using Listings.Application.Abstractions;
using Listings.Application.GetListingById.Records;
using Listings.Application.GetListings.ReadModels;
using Listings.Application.GetListings.Records;
using Listings.Domain.Exceptions.ListingsExceptions;
using Microsoft.Extensions.Logging;
using UserAccess.Contracts.Users.Interfaces;

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
    public async Task<ListingResult> HandleAsync(
        GetListingByIdQuery query,
        CancellationToken cancellationToken)
    {
        if (query.ListingId == Guid.Empty)
        {
            throw new ArgumentException("LISTING_ID_REQUIRED");
        }

        _logger.LogInformation(
            "Starting public listing details query. ListingId: {ListingId}",
            query.ListingId);

        var listing = await _listingReadService.GetPublicByIdAsync(
            query.ListingId,
            cancellationToken);

        if (listing is null)
        {
            _logger.LogWarning(
                "Public listing details query failed because listing was not found or is unavailable. ListingId: {ListingId}",
                query.ListingId);

            throw new ListingNotFoundException();
        }

        var providerProfiles =
            await _userPublicProfileQueries.GetByIdsAsync(
                new[] { listing.OwnerId },
                cancellationToken);
        
        var provider = providerProfiles
            .FirstOrDefault(profile =>
                profile.UserId == listing.OwnerId);
        
        if (provider is null || !provider.IsActive)
        {
            throw new ListingNotFoundException();
        }

        var providerName = provider.FullName;

        var result = MapListing(
            listing,
            providerName);

        _logger.LogInformation(
            "Public listing details query completed successfully. ListingId: {ListingId}, OwnerId: {OwnerId}",
            listing.ListingId,
            listing.OwnerId);

        return result;
    }

    /// <summary>
    /// Maps the listing read model to the public result.
    /// / Mapeia o modelo de leitura para o resultado público.
    /// </summary>
    private ListingResult MapListing(
        ListingReadModel listing,
        string? providerName)
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
}