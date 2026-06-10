using Microsoft.Extensions.Logging;
using Rentals.Application.Abstractions;
using Rentals.Application.GetRentals.Enums;
using Rentals.Application.GetRentals.ReadModels;
using Rentals.Application.GetRentals.Records;
using Rentals.Domain.Exceptions.RentalsExceptions;
using UserAccess.Contracts.Users.Interfaces;

namespace Rentals.Application.GetRentals;

/// <summary>
/// Handles rental searches for the authenticated user.
/// / Manipula pesquisas de aluguéis do usuário autenticado.
/// </summary>
public sealed class GetRentalsHandler
{
    private const int MaximumPageSize = 50;

    private readonly IRentalReadService _rentalReadService;
    private readonly IUserPublicProfileQueries _userPublicProfileQueries;
    private readonly ILogger<GetRentalsHandler> _logger;

    public GetRentalsHandler(
        IRentalReadService rentalReadService,
        IUserPublicProfileQueries userPublicProfileQueries,
        ILogger<GetRentalsHandler> logger)
    {
        _rentalReadService = rentalReadService;
        _userPublicProfileQueries = userPublicProfileQueries;
        _logger = logger;
    }

    /// <summary>
    /// Searches rentals in which the authenticated user is provider or renter.
    /// / Pesquisa aluguéis nos quais o usuário autenticado é fornecedor ou locatário.
    /// </summary>
    public async Task<GetRentalsResult> HandleAsync(
        GetRentalsQuery query,
        CancellationToken cancellationToken)
    {
        ValidateQuery(query);

        var criteria = new RentalSearchCriteria(
            query.UserId,
            query.Role!.Value,
            query.Status,
            Skip: (query.Page - 1) * query.PageSize,
            Take: query.PageSize);

        var searchPage = await _rentalReadService.SearchAsync(
            criteria,
            cancellationToken);

        var userIds = searchPage.Items
            .SelectMany(rental => new[]
            {
                rental.ProviderId,
                rental.RenterId
            })
            .Distinct()
            .ToArray();

        var publicProfiles =
            await _userPublicProfileQueries.GetByIdsAsync(
                userIds,
                cancellationToken);

        var namesByUserId = publicProfiles
            .ToDictionary(
                profile => profile.UserId,
                profile => profile.FullName);

        var items = searchPage.Items
            .Select(rental =>
            {
                namesByUserId.TryGetValue(
                    rental.ProviderId,
                    out var providerName);

                namesByUserId.TryGetValue(
                    rental.RenterId,
                    out var renterName);

                return MapRental(
                    rental,
                    providerName,
                    renterName);
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
            "Rentals search completed. UserId: {UserId}, Role: {Role}, Status: {Status}, Page: {Page}, PageSize: {PageSize}, TotalCount: {TotalCount}",
            query.UserId,
            query.Role,
            query.Status,
            query.Page,
            query.PageSize,
            searchPage.TotalCount);

        return new GetRentalsResult(
            items,
            query.Page,
            query.PageSize,
            searchPage.TotalCount,
            totalPages);
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
            providerName?.ToUpperInvariant(),
            rental.RenterId,
            renterName?.ToUpperInvariant(),
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

    private static void ValidateQuery(GetRentalsQuery query)
    {
        if (query.UserId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "AUTHENTICATED_USER_REQUIRED");
        }

        if (query.Role is null)
        {
            throw new InvalidRentalRequestException(
                "RENTAL_ROLE_REQUIRED");
        }

        if (!Enum.IsDefined(
                typeof(RentalParticipantRole),
                query.Role.Value))
        {
            throw new InvalidRentalRequestException(
                "INVALID_RENTAL_ROLE");
        }

        if (!Enum.IsDefined(
                typeof(RentalStatusFilter),
                query.Status))
        {
            throw new InvalidRentalRequestException(
                "INVALID_RENTAL_STATUS_FILTER");
        }

        if (query.Page < 1)
        {
            throw new InvalidRentalRequestException(
                "PAGE_MUST_BE_GREATER_THAN_ZERO");
        }

        if (query.PageSize < 1 ||
            query.PageSize > MaximumPageSize)
        {
            throw new InvalidRentalRequestException(
                "PAGE_SIZE_MUST_BE_BETWEEN_1_AND_50");
        }
    }
}