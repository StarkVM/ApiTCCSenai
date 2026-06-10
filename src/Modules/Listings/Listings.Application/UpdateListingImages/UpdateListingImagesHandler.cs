using Listings.Application.Abstractions;
using Listings.Application.UpdateListingImages.Records;
using Listings.Domain.Exceptions.ListingsExceptions;
using Listings.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using UserAccess.Contracts.Users.Interfaces;

namespace Listings.Application.UpdateListingImages;

/// <summary>
/// Handles the replacement of listing images.
/// / Manipula a substituição das imagens de um anúncio.
/// </summary>
public sealed class UpdateListingImagesHandler
{
    private const int MaxImages = 5;

    private readonly IUserAccessQueries _userAccessQueries;
    private readonly IListingRepository _listingRepository;
    private readonly IListingImageStorage _listingImageStorage;
    private readonly IListingImageUrlProvider _listingImageUrlProvider;
    private readonly IClock _clock;
    private readonly ILogger<UpdateListingImagesHandler> _logger;

    public UpdateListingImagesHandler(
        IUserAccessQueries userAccessQueries,
        IListingRepository listingRepository,
        IListingImageStorage listingImageStorage,
        IListingImageUrlProvider listingImageUrlProvider,
        IClock clock,
        ILogger<UpdateListingImagesHandler> logger)
    {
        _userAccessQueries = userAccessQueries;
        _listingRepository = listingRepository;
        _listingImageStorage = listingImageStorage;
        _listingImageUrlProvider = listingImageUrlProvider;
        _clock = clock;
        _logger = logger;
    }

    public async Task<UpdateListingImagesResult> HandleAsync(
        UpdateListingImagesCommand command,
        CancellationToken cancellationToken)
    {
        ValidateCommand(command);

        _logger.LogInformation(
            "Starting update listing images flow. ListingId: {ListingId}, RequesterId: {RequesterId}, ImageCount: {ImageCount}",
            command.ListingId,
            command.RequesterId,
            command.Images.Count);

        var userAccessSnapshot =
            await _userAccessQueries.GetUserAccessSnapshotAsync(
                command.RequesterId,
                cancellationToken);

        if (userAccessSnapshot is null ||
            !userAccessSnapshot.IsActive ||
            !userAccessSnapshot.IsProvider)
        {
            throw new InvalidListingOwnerException();
        }

        var listing =
            await _listingRepository.GetByIdForImageUpdateAsync(
                command.ListingId,
                cancellationToken);

        if (listing is null ||
            listing.OwnerId != command.RequesterId)
        {
            throw new ListingNotFoundException();
        }

        var oldStorageKeys =
            await _listingRepository.GetImageStorageKeysAsync(
                listing.Id,
                cancellationToken);

        var uploadedStorageKeys = new List<string>();

        try
        {
            var displayOrder = 1;

            foreach (var image in command.Images)
            {
                var storageKey = await _listingImageStorage.UpdateAsync(
                    listing.Id,
                    image,
                    displayOrder,
                    cancellationToken);

                uploadedStorageKeys.Add(storageKey);

                displayOrder++;
            }

            var nowUtc = _clock.UtcNow;

            listing.MarkImagesReplaced(nowUtc);
            
            listing.Approve(nowUtc);

            var newImages =
                await _listingRepository.ReplaceImageRowsAndSaveAsync(
                    listing,
                    uploadedStorageKeys,
                    cancellationToken);

            await DeleteOldImagesBestEffortAsync(
                oldStorageKeys,
                cancellationToken);

            var images = newImages
                .OrderBy(image => image.DisplayOrder)
                .Select(image =>
                {
                    var accessUrl = _listingImageUrlProvider.Generate(
                        image.StorageKey);

                    return new UpdatedListingImageResult(
                        image.Id,
                        accessUrl.Url,
                        image.DisplayOrder,
                        accessUrl.ExpiresAtUtc);
                })
                .ToArray();

            _logger.LogInformation(
                "Update listing images flow completed successfully. ListingId: {ListingId}, Status: {Status}, ImageCount: {ImageCount}",
                listing.Id,
                listing.Status,
                images.Length);

            return new UpdateListingImagesResult(
                listing.Id,
                listing.Status,
                listing.UpdatedAtUtc,
                images);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Update listing images flow failed. ListingId: {ListingId}, RequesterId: {RequesterId}",
                command.ListingId,
                command.RequesterId);

            await DeleteUploadedImagesBestEffortAsync(
                uploadedStorageKeys,
                cancellationToken);

            throw;
        }
    }

    private static void ValidateCommand(
        UpdateListingImagesCommand command)
    {
        if (command.ListingId == Guid.Empty)
        {
            throw new ArgumentException("LISTING_ID_REQUIRED");
        }

        if (command.RequesterId == Guid.Empty)
        {
            throw new ArgumentException("REQUESTER_ID_REQUIRED");
        }

        if (command.Images.Count == 0)
        {
            throw new ArgumentException(
                "LISTING_MUST_HAVE_AT_LEAST_ONE_IMAGE");
        }

        if (command.Images.Count > MaxImages)
        {
            throw new ArgumentException(
                "LISTING_IMAGE_LIMIT_EXCEEDED");
        }

        foreach (var image in command.Images)
        {
            if (string.IsNullOrWhiteSpace(image.FileName))
            {
                throw new ArgumentException(
                    "LISTING_IMAGE_FILE_NAME_REQUIRED");
            }

            if (string.IsNullOrWhiteSpace(image.ContentType))
            {
                throw new ArgumentException(
                    "LISTING_IMAGE_CONTENT_TYPE_REQUIRED");
            }

            if (image.Length <= 0)
            {
                throw new ArgumentException(
                    "LISTING_IMAGE_EMPTY");
            }
        }
    }

    private async Task DeleteUploadedImagesBestEffortAsync(
        IReadOnlyCollection<string> storageKeys,
        CancellationToken cancellationToken)
    {
        foreach (var storageKey in storageKeys)
        {
            try
            {
                await _listingImageStorage.DeleteAsync(
                    storageKey,
                    cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Failed to delete newly uploaded listing image after update failure. StorageKey: {StorageKey}",
                    storageKey);
            }
        }
    }

    private async Task DeleteOldImagesBestEffortAsync(
        IReadOnlyCollection<string> storageKeys,
        CancellationToken cancellationToken)
    {
        foreach (var storageKey in storageKeys)
        {
            try
            {
                await _listingImageStorage.DeleteAsync(
                    storageKey,
                    cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Failed to delete old listing image after successful update. StorageKey: {StorageKey}",
                    storageKey);
            }
        }
    }
}