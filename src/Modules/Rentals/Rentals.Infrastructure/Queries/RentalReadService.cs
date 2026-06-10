using Microsoft.EntityFrameworkCore;
using Rentals.Application.Abstractions;
using Rentals.Application.GetRentals.Enums;
using Rentals.Application.GetRentals.ReadModels;
using Rentals.Domain.Enums;
using Rentals.Infrastructure.Persistence;

namespace Rentals.Infrastructure.Queries;

/// <summary>
/// EF Core implementation of optimized rental read operations.
/// / Implementação EF Core das operações otimizadas de leitura de aluguéis.
/// </summary>
public sealed class RentalReadService : IRentalReadService
{
    private readonly RentalsDbContext _rentalsDbContext;

    public RentalReadService(
        RentalsDbContext rentalsDbContext)
    {
        _rentalsDbContext = rentalsDbContext;
    }

    public async Task<RentalSearchPage> SearchAsync(
        RentalSearchCriteria criteria,
        CancellationToken cancellationToken)
    {
        var query = _rentalsDbContext.Rentals
            .AsNoTracking()
            .AsQueryable();

        query = criteria.Role switch
        {
            RentalParticipantRole.Provider =>
                query.Where(rental =>
                    rental.ProviderId == criteria.UserId),

            RentalParticipantRole.Renter =>
                query.Where(rental =>
                    rental.RenterId == criteria.UserId),

            _ => throw new ArgumentOutOfRangeException(
                nameof(criteria.Role),
                criteria.Role,
                "Unsupported rental participant role.")
        };

        query = criteria.Status switch
        {
            RentalStatusFilter.All => query,

            RentalStatusFilter.Active =>
                query.Where(rental =>
                    rental.Status == RentalStatus.Approved ||
                    rental.Status == RentalStatus.InProgress),

            RentalStatusFilter.Completed =>
                query.Where(rental =>
                    rental.Status == RentalStatus.Completed),

            RentalStatusFilter.Cancelled =>
                query.Where(rental =>
                    rental.Status == RentalStatus.Cancelled),

            _ => throw new ArgumentOutOfRangeException(
                nameof(criteria.Status),
                criteria.Status,
                "Unsupported rental status filter.")
        };

        var totalCount = await query.CountAsync(
            cancellationToken);

        var items = await query
            .OrderByDescending(rental => rental.CreatedAtUtc)
            .Skip(criteria.Skip)
            .Take(criteria.Take)
            .Select(rental => new RentalReadModel(
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
                rental.CompletedByUserId))
            .ToArrayAsync(cancellationToken);

        return new RentalSearchPage(
            items,
            totalCount);
    }
    
    public Task<RentalReadModel?> GetByIdForParticipantAsync(
        Guid rentalId,
        Guid participantId,
        CancellationToken cancellationToken)
    {
        if (rentalId == Guid.Empty ||
            participantId == Guid.Empty)
        {
            return Task.FromResult<RentalReadModel?>(null);
        }

        return _rentalsDbContext.Rentals
            .AsNoTracking()
            .Where(rental =>
                rental.Id == rentalId &&
                (
                    rental.ProviderId == participantId ||
                    rental.RenterId == participantId
                ))
            .Select(rental => new RentalReadModel(
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
                rental.CompletedByUserId))
            .SingleOrDefaultAsync(cancellationToken);
    }
}