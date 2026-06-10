using Microsoft.Extensions.Logging;
using Rentals.Application.Abstractions;
using Rentals.Application.GetRentalById.Records;
using Rentals.Application.GetRentals.ReadModels;
using Rentals.Application.GetRentals.Records;
using Rentals.Domain.Exceptions.RentalsExceptions;
using UserAccess.Contracts.Users.Interfaces;

namespace Rentals.Application.GetRentalById;

/// <summary>
/// Handles the protected rental details query.
/// / Manipula a consulta protegida dos detalhes de um aluguel.
/// </summary>
public sealed class GetRentalByIdHandler
{
    private readonly IRentalReadService _rentalReadService;
    private readonly IUserPublicProfileQueries _userPublicProfileQueries;
    private readonly ILogger<GetRentalByIdHandler> _logger;

    public GetRentalByIdHandler(
        IRentalReadService rentalReadService,
        IUserPublicProfileQueries userPublicProfileQueries,
        ILogger<GetRentalByIdHandler> logger)
    {
        _rentalReadService = rentalReadService;
        _userPublicProfileQueries = userPublicProfileQueries;
        _logger = logger;
    }
    
    public async Task<RentalResult> HandleAsync(
        GetRentalByIdQuery query,
        CancellationToken cancellationToken)
    {
        ValidateQuery(query);

        _logger.LogInformation(
            "Starting protected rental details query. RentalId: {RentalId}, RequesterId: {RequesterId}",
            query.RentalId,
            query.RequesterId);

        var rental =
            await _rentalReadService.GetByIdForParticipantAsync(
                query.RentalId,
                query.RequesterId,
                cancellationToken);
        
        if (rental is null)
        {
            _logger.LogWarning(
                "Protected rental details query failed because rental was not found or requester has no access. RentalId: {RentalId}, RequesterId: {RequesterId}",
                query.RentalId,
                query.RequesterId);

            throw new RentalNotFoundException();
        }

        var participantIds = new[]
        {
            rental.ProviderId,
            rental.RenterId
        }
        .Distinct()
        .ToArray();

        var profiles =
            await _userPublicProfileQueries.GetByIdsAsync(
                participantIds,
                cancellationToken);

        var namesByUserId = profiles
            .ToDictionary(
                profile => profile.UserId,
                profile => profile.FullName);

        namesByUserId.TryGetValue(
            rental.ProviderId,
            out var providerName);

        namesByUserId.TryGetValue(
            rental.RenterId,
            out var renterName);

        _logger.LogInformation(
            "Protected rental details query completed successfully. RentalId: {RentalId}, RequesterId: {RequesterId}",
            rental.RentalId,
            query.RequesterId);

        return MapRental(
            rental,
            providerName,
            renterName);
    }
    
    private static RentalResult MapRental(
        RentalReadModel rental,
        string? providerName,
        string? renterName)
    {
        return new RentalResult(
            rental.RentalId,
            rental.ListingId,
            rental.ProviderId,
            providerName,
            rental.RenterId,
            renterName,
            rental.Status,
            rental.StartDate,
            rental.EndDate,
            rental.TotalDays,
            rental.IncludeOperator,
            rental.IncludeFreight,
            rental.ListingDailyPriceSnapshot,
            rental.OperatorDailyPriceSnapshot,
            rental.FreightFixedPriceSnapshot,
            rental.MachineSubtotal,
            rental.OperatorSubtotal,
            rental.FreightSubtotal,
            rental.TotalAmount,
            rental.CreatedAtUtc,
            rental.ApprovedAtUtc,
            rental.UpdatedAtUtc,
            rental.StartedAtUtc,
            rental.CompletedAtUtc,
            rental.CancelledAtUtc,
            rental.CompletedByUserId);
    }

    private static void ValidateQuery(
        GetRentalByIdQuery query)
    {
        if (query.RentalId == Guid.Empty)
        {
            throw new ArgumentException(
                "RENTAL_ID_REQUIRED");
        }

        if (query.RequesterId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "AUTHENTICATED_USER_REQUIRED");
        }
    }
}