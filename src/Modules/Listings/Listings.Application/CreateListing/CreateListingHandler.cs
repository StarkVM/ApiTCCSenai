using Microsoft.Extensions.Logging;

using Listings.Application.CreateListings.Records;
using Listings.Domain.Entities;
using Listings.Domain.Exceptions.ListingsExceptions;
using Listings.Domain.Interfaces;
using Listings.Domain.ValueObjects;
using UserAccess.Contracts.Users.Interfaces;

namespace Listings.Application.CreateListing;

/// <summary>
/// Handles the create listing use case.
/// / Manipula o caso de uso de criação de anúncio.
/// </summary>
public sealed class CreateListingHandler
{
    private static readonly string[] AllowedContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];

    private static readonly string[] AllowedExtensions =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    ];

    private readonly IUserAccessQueries _userAccessQueries;
    private readonly IListingRepository _listingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IListingImageStorage _listingImageStorage;
    private readonly IClock _clock;
    private readonly ILogger<CreateListingHandler> _logger;

    public CreateListingHandler(
        IUserAccessQueries userAccessQueries,
        IListingRepository listingRepository,
        IUnitOfWork unitOfWork,
        IListingImageStorage listingImageStorage,
        IClock clock,
        ILogger<CreateListingHandler> logger)
    {
        _userAccessQueries = userAccessQueries;
        _listingRepository = listingRepository;
        _unitOfWork = unitOfWork;
        _listingImageStorage = listingImageStorage;
        _clock = clock;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new listing after validating the owner and uploading images.
    /// / Cria um novo anúncio após validar o dono e enviar as imagens.
    /// </summary>
    public async Task<CreateListingResult> HandleAsync(
        CreateListingCommand command,
        CancellationToken cancellationToken)
    {
        var nowUtc = _clock.UtcNow;

        _logger.LogInformation(
            "Starting create listing flow. OwnerId: {OwnerId}, ImagesCount: {ImagesCount}",
            command.OwnerId,
            command.Images.Count);

        if (command.OwnerId == Guid.Empty)
        {
            _logger.LogWarning(
                "Create listing failed because owner id is empty.");

            throw new ArgumentException("OWNER_ID_REQUIRED");
        }

        var userAccessSnapshot = await _userAccessQueries.GetUserAccessSnapshotAsync(
            command.OwnerId,
            cancellationToken);

        if (userAccessSnapshot is null)
        {
            _logger.LogWarning(
                "Create listing failed because owner was not found. OwnerId: {OwnerId}",
                command.OwnerId);

            throw new InvalidListingOwnerException();
        }

        if (!userAccessSnapshot.IsActive)
        {
            _logger.LogWarning(
                "Create listing failed because owner is not active. OwnerId: {OwnerId}",
                command.OwnerId);

            throw new InvalidListingOwnerException();
        }

        if (!userAccessSnapshot.IsProvider)
        {
            _logger.LogWarning(
                "Create listing failed because owner is not provider. OwnerId: {OwnerId}",
                command.OwnerId);

            throw new InvalidListingOwnerException();
        }
        
        if (command.Images.Count == 0)
        {
            _logger.LogWarning(
                "Create listing failed because no images were sent. OwnerId: {OwnerId}",
                command.OwnerId);

            throw new InvalidListingImagesException("AT_LEAST_ONE_IMAGE_REQUIRED");
        }

        if (!Listing.ListingImageSizeIsValid(command.Images.Count))
        {
            _logger.LogWarning(
                "Create listing failed because too many images were sent. OwnerId: {OwnerId}, ImagesCount: {ImagesCount}",
                command.OwnerId,
                command.Images.Count);

            throw new InvalidListingImagesException("MAXIMUM_OF_FIVE_IMAGES_ALLOWED");
        }

        var listingId = Guid.NewGuid();
        var createdAtUtc = nowUtc;

        _logger.LogInformation(
            "Owner validated successfully for create listing flow. OwnerId: {OwnerId}, ListingId: {ListingId}",
            command.OwnerId,
            listingId);

        var pickupAddress = new PickupAddress(
            command.PickupAddress.State,
            command.PickupAddress.City,
            command.PickupAddress.District,
            command.PickupAddress.Street,
            command.PickupAddress.Number,
            command.PickupAddress.ZipCode,
            command.PickupAddress.Complement);

        OperatorOption operatorOption;

        if (command.OperatorOption.IsAvailable)
        {
            operatorOption = OperatorOption.Available(
                command.OperatorOption.AdditionalDailyPrice);
        }
        else
        {
            operatorOption = OperatorOption.NotAvailable();
        }

        FreightOption freightOption;

        if (command.FreightOption.IsAvailable)
        {
            freightOption = FreightOption.Available(
                command.FreightOption.FixedPrice);
        }
        else
        {
            freightOption = FreightOption.NotAvailable();
        }

        var listing = new Listing(
            listingId,
            command.OwnerId,
            command.Title,
            command.Description,
            command.Category,
            command.DailyPrice,
            pickupAddress,
            operatorOption,
            freightOption,
            createdAtUtc,
            command.IsFleet
        );
        
        _logger.LogInformation(
            "Listing entity created in memory. ListingId: {ListingId}, OwnerId: {OwnerId}",
            listing.Id,
            command.OwnerId);

        var uploadedStorageKeys = new List<string>();

        try
        {
            var displayOrder = 1;

            foreach (var imageCommand in command.Images)
            {
                _logger.LogInformation(
                    "Starting listing image validation. ListingId: {ListingId}, DisplayOrder: {DisplayOrder}, ContentType: {ContentType}, Length: {Length}",
                    listingId,
                    displayOrder,
                    imageCommand.ContentType,
                    imageCommand.Length);

                ValidateImage(imageCommand);

                var imageId = Guid.NewGuid();

                _logger.LogInformation(
                    "Starting listing image upload. ListingId: {ListingId}, ImageId: {ImageId}, DisplayOrder: {DisplayOrder}",
                    listingId,
                    imageId,
                    displayOrder);

                var storageKey = await _listingImageStorage.UploadAsync(
                    listingId,
                    imageId,
                    imageCommand.FileName,
                    imageCommand.ContentType,
                    imageCommand.ContentStream,
                    cancellationToken);

                uploadedStorageKeys.Add(storageKey);

                _logger.LogInformation(
                    "Listing image uploaded successfully. ListingId: {ListingId}, ImageId: {ImageId}, StorageKey: {StorageKey}, DisplayOrder: {DisplayOrder}",
                    listingId,
                    imageId,
                    storageKey,
                    displayOrder);

                listing.AddImage(
                    imageId,
                    storageKey,
                    displayOrder,
                    createdAtUtc);

                displayOrder++;
            }

            await _listingRepository.AddAsync(
                listing,
                cancellationToken);

            _logger.LogInformation(
                "Listing added to repository. ListingId: {ListingId}, OwnerId: {OwnerId}",
                listing.Id,
                command.OwnerId);
            

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Create listing flow completed successfully. ListingId: {ListingId}, OwnerId: {OwnerId}, ImagesCount: {ImagesCount}",
                listing.Id,
                command.OwnerId,
                listing.Images.Count);

            return new CreateListingResult(
                listing.Id,
                listing.Status,
                listing.CreatedAtUtc);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Create listing flow failed. ListingId: {ListingId}, OwnerId: {OwnerId}, UploadedImagesCount: {UploadedImagesCount}",
                listingId,
                command.OwnerId,
                uploadedStorageKeys.Count);

            await DeleteUploadedImagesAsync(uploadedStorageKeys);

            throw;
        }
    }

    private static void ValidateImage(CreateListingImageCommand imageCommand)
    {
        if (string.IsNullOrWhiteSpace(imageCommand.FileName))
        {
            throw new InvalidListingImagesException("IMAGE_FILE_NAME_REQUIRED");
        }

        if (string.IsNullOrWhiteSpace(imageCommand.ContentType))
        {
            throw new InvalidListingImagesException("IMAGE_CONTENT_TYPE_REQUIRED");
        }

        if (imageCommand.Length <= 0)
        {
            throw new InvalidListingImagesException("IMAGE_EMPTY");
        }

        if (!Listing.ListingImageSizeIsValid(imageCommand.Length))
        {
            throw new InvalidListingImagesException("IMAGE_TOO_LARGE");
        }

        if (!AllowedContentTypes.Contains(imageCommand.ContentType))
        {
            throw new InvalidListingImagesException("IMAGE_CONTENT_TYPE_NOT_ALLOWED");
        }

        var extension = Path.GetExtension(imageCommand.FileName).ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension))
        {
            throw new InvalidListingImagesException("IMAGE_EXTENSION_NOT_ALLOWED");
        }

        if (imageCommand.ContentStream is null)
        {
            throw new InvalidListingImagesException("IMAGE_STREAM_REQUIRED");
        }
    }

    private async Task DeleteUploadedImagesAsync(IReadOnlyCollection<string> uploadedStorageKeys)
    {
        if (uploadedStorageKeys.Count == 0)
        {
            _logger.LogInformation(
                "No uploaded listing images to delete after create listing failure.");

            return;
        }

        _logger.LogWarning(
            "Starting uploaded listing images cleanup. UploadedImagesCount: {UploadedImagesCount}",
            uploadedStorageKeys.Count);

        foreach (var storageKey in uploadedStorageKeys)
        {
            try
            {
                await _listingImageStorage.DeleteAsync(
                    storageKey,
                    CancellationToken.None);

                _logger.LogInformation(
                    "Uploaded listing image deleted successfully during cleanup. StorageKey: {StorageKey}",
                    storageKey);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to delete uploaded listing image during cleanup. StorageKey: {StorageKey}",
                    storageKey);

                // Intentionally ignored for now.
                // / Ignorado intencionalmente por enquanto.
                //
                // Later we can add structured logging here.
                // / Depois podemos adicionar logging estruturado aqui.
            }
        }
    }
}