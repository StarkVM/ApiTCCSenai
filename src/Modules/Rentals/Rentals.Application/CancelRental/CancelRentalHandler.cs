using Listings.Contracts.Listings.Interfaces;
using Microsoft.Extensions.Logging;
using Rentals.Application.CancelRental.Records;
using Rentals.Application.Common.Exceptions;
using Rentals.Domain.Enums;
using Rentals.Domain.Exceptions.RentalsExceptions;
using Rentals.Domain.Interfaces;

namespace Rentals.Application.CancelRental;

/// <summary>
/// Handles the rental cancellation use case.
/// / Manipula o caso de uso de cancelamento de aluguel.
/// </summary>
public sealed class CancelRentalHandler
{
    private readonly IRentalRepository _rentalRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IListingRentalCommands _listingRentalCommands;
    private readonly IClock _clock;
    private readonly ILogger<CancelRentalHandler> _logger;

    public CancelRentalHandler(
        IRentalRepository rentalRepository,
        IUnitOfWork unitOfWork,
        IListingRentalCommands listingRentalCommands,
        IClock clock,
        ILogger<CancelRentalHandler> logger)
    {
        _rentalRepository = rentalRepository;
        _unitOfWork = unitOfWork;
        _listingRentalCommands = listingRentalCommands;
        _clock = clock;
        _logger = logger;
    }
    
    public async Task<CancelRentalResult> HandleAsync(
        CancelRentalCommand command,
        CancellationToken cancellationToken)
    {
        ValidateCommand(command);

        _logger.LogInformation(
            "Starting cancel rental flow. RentalId: {RentalId}, RequesterId: {RequesterId}",
            command.RentalId,
            command.RequesterId);

        var rental = await _rentalRepository.GetByIdAsync(
            command.RentalId,
            cancellationToken);

        if (rental is null)
        {
            _logger.LogWarning(
                "Cancel rental failed because rental was not found. RentalId: {RentalId}, RequesterId: {RequesterId}",
                command.RentalId,
                command.RequesterId);

            throw new RentalNotFoundException();
        }

        if (command.RequesterId != rental.ProviderId &&
            command.RequesterId != rental.RenterId)
        {
            _logger.LogWarning(
                "Cancel rental failed because requester is not a participant. RentalId: {RentalId}, ProviderId: {ProviderId}, RenterId: {RenterId}, RequesterId: {RequesterId}",
                rental.Id,
                rental.ProviderId,
                rental.RenterId,
                command.RequesterId);

            throw new UnauthorizedRentalParticipantException();
        }

        if (rental.Status == RentalStatus.Completed)
        {
            _logger.LogWarning(
                "Cancel rental failed because rental is completed. RentalId: {RentalId}",
                rental.Id);

            throw new InvalidRentalStatusTransitionException(
                "COMPLETED_RENTAL_CANNOT_BE_CANCELLED");
        }

        var nowUtc = _clock.UtcNow;

        try
        {
            rental.Cancel(
                command.RequesterId,
                nowUtc);
        }
        catch (InvalidOperationException exception)
        {
            throw new InvalidRentalStatusTransitionException(
                exception.Message);
        }

        /*
         * The rental is persisted before releasing the listing.
         * / O aluguel é persistido antes da liberação do anúncio.
         */
        try
        {
            await _unitOfWork.SaveChangesAsync(
                cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Cancel rental failed while saving rental changes. RentalId: {RentalId}",
                rental.Id);

            throw new DatabaseSaveFailedException(exception);
        }

        /*
         * This operation is also executed when the rental was already
         * cancelled, allowing retries to repair a suspended listing.
         *
         * / Esta operação também é executada quando o aluguel já estava
         * cancelado, permitindo que novas tentativas liberem um anúncio suspenso.
         */
        var listingWasReleased =
            await _listingRentalCommands.TryReleaseListingAfterRentalAsync(
                rental.ListingId,
                nowUtc,
                cancellationToken);

        if (!listingWasReleased)
        {
            _logger.LogCritical(
                "Rental was cancelled, but listing could not be released. RentalId: {RentalId}, ListingId: {ListingId}",
                rental.Id,
                rental.ListingId);
        }

        _logger.LogInformation(
            "Cancel rental flow completed successfully. RentalId: {RentalId}, ListingId: {ListingId}, CancelledByUserId: {CancelledByUserId}, PenaltyAmount: {PenaltyAmount}, ListingReleased: {ListingReleased}",
            rental.Id,
            rental.ListingId,
            rental.CancelledByUserId,
            rental.CancellationPenaltyAmount,
            listingWasReleased);

        return new CancelRentalResult(
            rental.Id,
            rental.ListingId,
            rental.ProviderId,
            rental.RenterId,
            rental.Status,
            rental.CancelledByUserId!.Value,
            rental.CancellationPenaltyAmount,
            rental.CancelledAtUtc!.Value);
    }

    private static void ValidateCommand(
        CancelRentalCommand command)
    {
        if (command.RentalId == Guid.Empty)
        {
            throw new InvalidRentalRequestException(
                "RENTAL_ID_REQUIRED");
        }

        if (command.RequesterId == Guid.Empty)
        {
            throw new InvalidRentalRequestException(
                "REQUESTER_ID_REQUIRED");
        }
    }
}