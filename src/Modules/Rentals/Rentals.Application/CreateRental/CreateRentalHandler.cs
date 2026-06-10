using Listings.Contracts.Listings.Enums;
using Listings.Contracts.Listings.Interfaces;
using Microsoft.Extensions.Logging;
using Rentals.Application.Common.Exceptions;
using Rentals.Application.CreateRental.Records;
using Rentals.Domain.Entities;
using Rentals.Domain.Exceptions.RentalsExceptions;
using Rentals.Domain.Interfaces;
using UserAccess.Contracts.Users.Interfaces;

namespace Rentals.Application.CreateRental;

/// <summary>
/// Handles the creation of approved rentals.
/// / Manipula a criação de aluguéis aprovados.
/// </summary>
public sealed class CreateRentalHandler
{
    private readonly IRentalRepository _rentalRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly IUserAccessQueries _userAccessQueries;
    private readonly IListingRentalQueries _listingRentalQueries;
    private readonly IListingRentalCommands _listingRentalCommands;
    private readonly ILogger<CreateRentalHandler> _logger;

    public CreateRentalHandler(
        IRentalRepository rentalRepository,
        IUnitOfWork unitOfWork,
        IClock clock,
        IUserAccessQueries userAccessQueries,
        IListingRentalQueries listingRentalQueries,
        IListingRentalCommands listingRentalCommands,
        ILogger<CreateRentalHandler> logger)
    {
        _rentalRepository = rentalRepository;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _userAccessQueries = userAccessQueries;
        _listingRentalQueries = listingRentalQueries;
        _listingRentalCommands = listingRentalCommands;
        _logger = logger;
    }

    /// <summary>
    /// Creates an approved rental based on a listing.
    /// / Cria um aluguel aprovado com base em um anúncio.
    /// </summary>
    public async Task<CreateRentalResult> HandleAsync(
        CreateRentalCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Starting create rental flow. ListingId: {ListingId}, RenterId: {RenterId}, StartDate: {StartDate}, EndDate: {EndDate}, IncludeOperator: {IncludeOperator}, IncludeFreight: {IncludeFreight}",
            command.ListingId,
            command.RenterId,
            command.StartDate,
            command.EndDate,
            command.IncludeOperator,
            command.IncludeFreight);

        ValidateCommand(command);

        var nowUtc = _clock.UtcNow;
        var today = DateOnly.FromDateTime(nowUtc);

        ValidateRentalPeriod(
            command.StartDate,
            command.EndDate,
            today);

        var renter = await _userAccessQueries.GetUserAccessSnapshotAsync(
            command.RenterId,
            cancellationToken);

        if (renter is null)
        {
            _logger.LogWarning(
                "Create rental failed because renter was not found. RenterId: {RenterId}",
                command.RenterId);

            throw new InvalidRentalRenterException("RENTER_NOT_FOUND");
        }

        if (!renter.IsActive)
        {
            _logger.LogWarning(
                "Create rental failed because renter is not active. RenterId: {RenterId}",
                command.RenterId);

            throw new InvalidRentalRenterException("RENTER_IS_NOT_ACTIVE");
        }

        var listing = await _listingRentalQueries.GetListingForRentalAsync(
            command.ListingId,
            cancellationToken);

        if (listing is null)
        {
            _logger.LogWarning(
                "Create rental failed because listing was not found. ListingId: {ListingId}, RenterId: {RenterId}",
                command.ListingId,
                command.RenterId);

            throw new RentalListingNotFoundException();
        }

        if (listing.Status != ListingContractStatus.Approved)
        {
            _logger.LogWarning(
                "Create rental failed because listing is not approved. ListingId: {ListingId}, ListingStatus: {ListingStatus}, RenterId: {RenterId}",
                listing.ListingId,
                listing.Status,
                command.RenterId);

            throw new ListingUnavailableForRentalException("LISTING_IS_NOT_AVAILABLE");
        }

        if (listing.OwnerId == command.RenterId)
        {
            _logger.LogWarning(
                "Create rental failed because renter is the listing owner. ListingId: {ListingId}, OwnerId: {OwnerId}, RenterId: {RenterId}",
                listing.ListingId,
                listing.OwnerId,
                command.RenterId);

            throw new CannotRentOwnListingException();
        }

        if (command.IncludeOperator && !listing.OperatorAvailable)
        {
            _logger.LogWarning(
                "Create rental failed because operator was requested but listing does not offer operator. ListingId: {ListingId}, RenterId: {RenterId}",
                listing.ListingId,
                command.RenterId);

            throw new RentalOptionUnavailableException("OPERATOR_NOT_AVAILABLE_FOR_LISTING");
        }

        if (command.IncludeFreight && !listing.FreightAvailable)
        {
            _logger.LogWarning(
                "Create rental failed because freight was requested but listing does not offer freight. ListingId: {ListingId}, RenterId: {RenterId}",
                listing.ListingId,
                command.RenterId);

            throw new RentalOptionUnavailableException("FREIGHT_NOT_AVAILABLE_FOR_LISTING");
        }

        if (!listing.IsFleet)
        {
            var hasActiveRental = await _rentalRepository.ExistsActiveRentalForListingAsync(
                listing.ListingId,
                cancellationToken);

            if (hasActiveRental)
            {
                _logger.LogWarning(
                    "Create rental failed because listing already has an active rental. ListingId: {ListingId}, RenterId: {RenterId}",
                    listing.ListingId,
                    command.RenterId);

                throw new ListingAlreadyHasActiveRentalException();
            }
        }

        
        var rentalId = Guid.NewGuid();

        decimal operatorDailyPrice;

        if (command.IncludeOperator)
        {
            operatorDailyPrice = listing.OperatorDailyPrice;
        }
        else
        {
            operatorDailyPrice = 0m;
        }

        decimal freightFixedPrice;

        if (command.IncludeFreight)
        {
            freightFixedPrice = listing.FreightFixedPrice;
        }
        else
        {
            freightFixedPrice = 0m;
        }

        var rental = Rental.CreateApproved(
            rentalId,
            listing.ListingId,
            listing.OwnerId,
            command.RenterId,
            command.StartDate,
            command.EndDate,
            command.IncludeOperator,
            command.IncludeFreight,
            listing.DailyPrice,
            operatorDailyPrice,
            freightFixedPrice,
            nowUtc);

        await _rentalRepository.AddAsync(
            rental,
            cancellationToken);

        if (!listing.IsFleet)
        {
            var listingWasSuspended =
                await _listingRentalCommands.TrySuspendListingForRentalAsync(
                    listing.ListingId,
                    nowUtc,
                    cancellationToken);

            if (!listingWasSuspended)
            {
                _logger.LogWarning(
                    "Create rental failed because listing could not be suspended. ListingId: {ListingId}, RenterId: {RenterId}",
                    listing.ListingId,
                    command.RenterId);

                throw new ListingUnavailableForRentalException(
                    "LISTING_COULD_NOT_BE_SUSPENDED");
            }

            _logger.LogInformation(
                "Listing suspended after rental creation because it is not a fleet listing. ListingId: {ListingId}, RentalId: {RentalId}",
                listing.ListingId,
                rental.Id);
        }

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Create rental flow completed successfully. RentalId: {RentalId}, ListingId: {ListingId}, OwnerId: {OwnerId}, RenterId: {RenterId}, TotalAmount: {TotalAmount}",
                rental.Id,
                rental.ListingId,
                rental.ProviderId,
                rental.RenterId,
                rental.TotalAmount);
        }
        catch (Exception ex)
        {
            
            throw new DatabaseSaveFailedException(ex);
        }
        

        return new CreateRentalResult(
            rental.Id,
            rental.ListingId,
            rental.ProviderId,
            rental.RenterId,
            rental.Status,
            rental.StartDate,
            rental.EndDate,
            rental.TotalDays,
            rental.IncludeOperator,
            rental.IncludeFreight,
            rental.MachineSubtotal,
            rental.OperatorSubtotal,
            rental.FreightSubtotal,
            rental.TotalAmount,
            rental.CreatedAtUtc);
    }

    private static void ValidateCommand(CreateRentalCommand command)
    {
        if (command.ListingId == Guid.Empty)
        {
            throw new InvalidRentalRequestException("LISTING_ID_REQUIRED");
        }

        if (command.RenterId == Guid.Empty)
        {
            throw new InvalidRentalRequestException("RENTER_ID_REQUIRED");
        }
    }

    private static void ValidateRentalPeriod(
        DateOnly startDate,
        DateOnly endDate,
        DateOnly today)
    {
        if (startDate < today)
        {
            throw new InvalidRentalPeriodException("START_DATE_CANNOT_BE_IN_THE_PAST");
        }

        if (endDate < startDate)
        {
            throw new InvalidRentalPeriodException("END_DATE_CANNOT_BE_BEFORE_START_DATE");
        }
    }

    private static int CalculateTotalDays(
        DateOnly startDate,
        DateOnly endDate)
    {
        return endDate.DayNumber - startDate.DayNumber + 1;
    }
}