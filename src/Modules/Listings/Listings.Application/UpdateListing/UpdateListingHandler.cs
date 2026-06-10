using Listings.Application.Common.Exceptions;
using Listings.Application.UpdateListing.Records;
using Listings.Domain.Enums;
using Listings.Domain.Exceptions.ListingsExceptions;
using Listings.Domain.Interfaces;
using Listings.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using UserAccess.Contracts.Users.Interfaces;

namespace Listings.Application.UpdateListing;

/// <summary>
/// Handles the listing update use case.
/// / Manipula o caso de uso de atualização de anúncio.
/// </summary>
public sealed class UpdateListingHandler
{
    private readonly IUserAccessQueries _userAccessQueries;
    private readonly IListingRepository _listingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly ILogger<UpdateListingHandler> _logger;

    public UpdateListingHandler(
        IUserAccessQueries userAccessQueries,
        IListingRepository listingRepository,
        IUnitOfWork unitOfWork,
        IClock clock,
        ILogger<UpdateListingHandler> logger)
    {
        _userAccessQueries = userAccessQueries;
        _listingRepository = listingRepository;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _logger = logger;
    }
    
    public async Task<UpdateListingResult> HandleAsync(
        UpdateListingCommand command,
        CancellationToken cancellationToken)
    {
        ValidateCommand(command);

        _logger.LogInformation(
            "Starting update listing flow. ListingId: {ListingId}, RequesterId: {RequesterId}",
            command.ListingId,
            command.RequesterId);

        var userAccessSnapshot =
            await _userAccessQueries.GetUserAccessSnapshotAsync(
                command.RequesterId,
                cancellationToken);

        if (userAccessSnapshot is null ||
            !userAccessSnapshot.IsActive ||
            !userAccessSnapshot.IsProvider)
        {
            _logger.LogWarning(
                "Update listing failed because requester is not an active provider. ListingId: {ListingId}, RequesterId: {RequesterId}",
                command.ListingId,
                command.RequesterId);

            throw new InvalidListingOwnerException();
        }

        var listing = await _listingRepository.GetByIdAsync(
            command.ListingId,
            cancellationToken);

        /*
         * Returns not found both when the listing does not exist and when
         * the requester does not own it.
         *
         * / Retorna não encontrado tanto quando o anúncio não existe quanto
         * quando o solicitante não é seu proprietário.
         */
        if (listing is null ||
            listing.OwnerId != command.RequesterId)
        {
            _logger.LogWarning(
                "Update listing failed because listing was not found or requester is not its owner. ListingId: {ListingId}, RequesterId: {RequesterId}",
                command.ListingId,
                command.RequesterId);

            throw new ListingNotFoundException();
        }

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

        var nowUtc = _clock.UtcNow;

        listing.UpdateDetails(
            command.Title,
            command.Description,
            command.Category,
            command.DailyPrice,
            pickupAddress,
            operatorOption,
            freightOption,
            nowUtc);
        
        listing.Approve(nowUtc);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new DatabaseSaveFailedException(ex);
        }

        _logger.LogInformation(
            "Update listing flow completed successfully. ListingId: {ListingId}, RequesterId: {RequesterId}, Status: {Status}",
            listing.Id,
            command.RequesterId,
            listing.Status);

        return new UpdateListingResult(
            listing.Id,
            listing.Status,
            listing.UpdatedAtUtc);
    }

    private static void ValidateCommand(
        UpdateListingCommand command)
    {
        if (command.ListingId == Guid.Empty)
        {
            throw new ArgumentException("LISTING_ID_REQUIRED");
        }

        if (command.RequesterId == Guid.Empty)
        {
            throw new ArgumentException("REQUESTER_ID_REQUIRED");
        }

        ArgumentNullException.ThrowIfNull(command.PickupAddress);
        ArgumentNullException.ThrowIfNull(command.OperatorOption);
        ArgumentNullException.ThrowIfNull(command.FreightOption);
    }
}
