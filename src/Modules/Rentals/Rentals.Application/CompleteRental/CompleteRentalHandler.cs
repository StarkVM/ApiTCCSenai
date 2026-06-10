using Listings.Contracts.Listings.Interfaces;
using Microsoft.Extensions.Logging;
using Rentals.Application.Common.Exceptions;
using Rentals.Application.CompleteRental.Records;
using Rentals.Domain.Enums;
using Rentals.Domain.Exceptions.RentalsExceptions;
using Rentals.Domain.Interfaces;

namespace Rentals.Application.CompleteRental;

/// <summary>
/// Handles the rental completion use case.
/// / Manipula o caso de uso de finalização de aluguel.
/// </summary>
public sealed class CompleteRentalHandler
{
    private readonly IRentalRepository _rentalRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IListingRentalCommands _listingRentalCommands;
    private readonly IClock _clock;
    private readonly ILogger<CompleteRentalHandler> _logger;

    public CompleteRentalHandler(
        IRentalRepository rentalRepository,
        IUnitOfWork unitOfWork,
        IListingRentalCommands listingRentalCommands,
        IClock clock,
        ILogger<CompleteRentalHandler> logger)
    {
        _rentalRepository = rentalRepository;
        _unitOfWork = unitOfWork;
        _listingRentalCommands = listingRentalCommands;
        _clock = clock;
        _logger = logger;
    }
    
    public async Task<CompleteRentalResult> HandleAsync(
        CompleteRentalCommand command,
        CancellationToken cancellationToken)
    {
        ValidateCommand(command);

        _logger.LogInformation(
            "Starting complete rental flow. RentalId: {RentalId}, RequesterId: {RequesterId}",
            command.RentalId,
            command.RequesterId);

        var rental = await _rentalRepository.GetByIdAsync(
            command.RentalId,
            cancellationToken);

        if (rental is null)
        {
            _logger.LogWarning(
                "Complete rental failed because rental was not found. RentalId: {RentalId}, RequesterId: {RequesterId}",
                command.RentalId,
                command.RequesterId);

            throw new RentalNotFoundException();
        }

        if (command.RequesterId != rental.ProviderId &&
            command.RequesterId != rental.RenterId)
        {
            _logger.LogWarning(
                "Complete rental failed because requester is not a participant. RentalId: {RentalId}, ProviderId: {ProviderId}, RenterId: {RenterId}, RequesterId: {RequesterId}",
                rental.Id,
                rental.ProviderId,
                rental.RenterId,
                command.RequesterId);

            throw new UnauthorizedRentalParticipantException();
        }

        if (rental.Status == RentalStatus.Cancelled)
        {
            throw new InvalidRentalStatusTransitionException(
                "CANCELLED_RENTAL_CANNOT_BE_COMPLETED");
        }

        var nowUtc = _clock.UtcNow;

        try
        {
            rental.Complete(
                command.RequesterId,
                nowUtc);
        }
        catch (InvalidOperationException exception)
        {
            throw new InvalidRentalStatusTransitionException(
                exception.Message);
        }

        /*
         * The rental is persisted first.
         * If listing release fails, the machine remains unavailable,
         * which is safer than making an actively rented machine available.
         *
         * / O aluguel é persistido primeiro.
         * Se a liberação do anúncio falhar, a máquina permanece indisponível,
         * o que é mais seguro do que disponibilizar uma máquina ainda alugada.
         */
        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Complete rental failed while saving rental changes. RentalId: {RentalId}",
                rental.Id);

            throw new DatabaseSaveFailedException(exception);
        }

        var listingWasReleased =
            await _listingRentalCommands.TryReleaseListingAfterRentalAsync(
                rental.ListingId,
                nowUtc,
                cancellationToken);

        if (!listingWasReleased)
        {
            /*
             * The rental is already completed.
             * Do not reverse its state automatically.
             * A retry or repair process can release the listing later.
             *
             * / O aluguel já foi finalizado.
             * Não reverta seu estado automaticamente.
             * Um processo de repetição ou reparo poderá liberar o anúncio depois.
             */
            _logger.LogCritical(
                "Rental was completed, but listing could not be released. RentalId: {RentalId}, ListingId: {ListingId}",
                rental.Id,
                rental.ListingId);
        }

        _logger.LogInformation(
            "Complete rental flow completed successfully. RentalId: {RentalId}, ListingId: {ListingId}, CompletedByUserId: {CompletedByUserId}, ListingReleased: {ListingReleased}",
            rental.Id,
            rental.ListingId,
            rental.CompletedByUserId,
            listingWasReleased);

        return new CompleteRentalResult(
            rental.Id,
            rental.ListingId,
            rental.ProviderId,
            rental.RenterId,
            rental.Status,
            rental.CompletedByUserId!.Value,
            rental.CompletedAtUtc!.Value);
    }

    private static void ValidateCommand(
        CompleteRentalCommand command)
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