using Rentals.Domain.Enums;

namespace Rentals.Domain.Entities;

/// <summary>
/// Represents a machinery rental created from a listing.
/// / Representa um aluguel de máquina criado a partir de um anúncio.
/// </summary>
public sealed class Rental
{
    
    public Guid Id { get; private set; }
    public Guid ListingId { get; private set; }
    public Guid ProviderId { get; private set; }
    public Guid RenterId { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public int TotalDays { get; private set; }
    public bool IncludeOperator { get; private set; }
    public bool IncludeFreight { get; private set; }
    public decimal ListingDailyPriceSnapshot { get; private set; }
    public decimal OperatorDailyPriceSnapshot { get; private set; }
    public decimal FreightFixedPriceSnapshot { get; private set; }
    public decimal MachineSubtotal { get; private set; }
    public decimal OperatorSubtotal { get; private set; }
    public decimal FreightSubtotal { get; private set; }
    public decimal TotalAmount { get; private set; }
    public RentalStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime ApprovedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public Guid? CompletedByUserId { get; private set; }
    
    public Guid? CancelledByUserId { get; private set; }

    public decimal CancellationPenaltyAmount { get; private set; }
    public DateTime? StartedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }

    private Rental()
    {
    }

    private Rental(
        Guid id,
        Guid listingId,
        Guid providerId,
        Guid renterId,
        DateOnly startDate,
        DateOnly endDate,
        bool includeOperator,
        bool includeFreight,
        decimal listingDailyPrice,
        decimal operatorDailyPrice,
        decimal freightFixedPrice,
        DateTime createdAtUtc)
    {
        Id = id;
        ListingId = listingId;
        ProviderId = providerId;
        RenterId = renterId;

        StartDate = startDate;
        EndDate = endDate;
        TotalDays = CalculateTotalDays(startDate, endDate);

        IncludeOperator = includeOperator;
        IncludeFreight = includeFreight;

        ListingDailyPriceSnapshot = listingDailyPrice;

        if (includeOperator)
        {
            OperatorDailyPriceSnapshot = operatorDailyPrice;
        }
        else
        {
            OperatorDailyPriceSnapshot = 0m;
        }

        if (includeFreight)
        {
            FreightFixedPriceSnapshot = freightFixedPrice;
        }
        else
        {
            FreightFixedPriceSnapshot = 0m;
        }

        MachineSubtotal =
            ListingDailyPriceSnapshot * TotalDays;

        if (includeOperator)
        {
            OperatorSubtotal =
                OperatorDailyPriceSnapshot * TotalDays;
        }
        else
        {
            OperatorSubtotal = 0m;
        }

        if (includeFreight)
        {
            FreightSubtotal = FreightFixedPriceSnapshot;
        }
        else
        {
            FreightSubtotal = 0m;
        }

        TotalAmount =
            MachineSubtotal +
            OperatorSubtotal +
            FreightSubtotal;

        Status = RentalStatus.Approved;

        CreatedAtUtc = createdAtUtc;
        ApprovedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }
    
    public static Rental CreateApproved(
        Guid id,
        Guid listingId,
        Guid ownerId,
        Guid renterId,
        DateOnly startDate,
        DateOnly endDate,
        bool includeOperator,
        bool includeFreight,
        decimal listingDailyPrice,
        decimal operatorDailyPrice,
        decimal freightFixedPrice,
        DateTime createdAtUtc)
    {
        ValidateIdentifiers(
            id,
            listingId,
            ownerId,
            renterId);

        ValidateRentalPeriod(
            startDate,
            endDate);

        ValidatePrices(
            includeOperator,
            includeFreight,
            listingDailyPrice,
            operatorDailyPrice,
            freightFixedPrice);

        if (ownerId == renterId)
        {
            throw new InvalidOperationException(
                "The listing owner cannot rent their own listing.");
        }

        return new Rental(
            id,
            listingId,
            ownerId,
            renterId,
            startDate,
            endDate,
            includeOperator,
            includeFreight,
            listingDailyPrice,
            operatorDailyPrice,
            freightFixedPrice,
            createdAtUtc);
    }
    
    private static int CalculateTotalDays(
        DateOnly startDate,
        DateOnly endDate)
    {
        return endDate.DayNumber - startDate.DayNumber + 1;
    }
    
    private static void ValidateIdentifiers(
        Guid id,
        Guid listingId,
        Guid ownerId,
        Guid renterId)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Rental id cannot be empty.");
        }

        if (listingId == Guid.Empty)
        {
            throw new ArgumentException("Listing id cannot be empty.");
        }

        if (ownerId == Guid.Empty)
        {
            throw new ArgumentException("Owner id cannot be empty.");
        }

        if (renterId == Guid.Empty)
        {
            throw new ArgumentException("Renter id cannot be empty.");
        }
    }
    
    private static void ValidateRentalPeriod(
        DateOnly startDate,
        DateOnly endDate)
    {
        if (endDate < startDate)
        {
            throw new ArgumentException(
                "Rental end date cannot be before the start date.");
        }
    }
    
    private static void ValidatePrices(
        bool includeOperator,
        bool includeFreight,
        decimal listingDailyPrice,
        decimal operatorDailyPrice,
        decimal freightFixedPrice)
    {
        if (listingDailyPrice <= 0)
        {
            throw new ArgumentException(
                "Listing daily price must be greater than zero.");
        }

        if (includeOperator && operatorDailyPrice <= 0)
        {
            throw new ArgumentException(
                "Operator daily price must be greater than zero when operator is included.");
        }

        if (!includeOperator && operatorDailyPrice < 0)
        {
            throw new ArgumentException(
                "Operator daily price cannot be negative.");
        }

        if (includeFreight && freightFixedPrice <= 0)
        {
            throw new ArgumentException(
                "Freight price must be greater than zero when freight is included.");
        }

        if (!includeFreight && freightFixedPrice < 0)
        {
            throw new ArgumentException(
                "Freight price cannot be negative.");
        }
    }
    
    public void Complete(
        Guid requesterId,
        DateTime completedAtUtc)
    {
        if (requesterId == Guid.Empty)
        {
            throw new ArgumentException(
                "Requester id cannot be empty.");
        }

        if (requesterId != ProviderId &&
            requesterId != RenterId)
        {
            throw new InvalidOperationException(
                "Only the rental provider or renter can complete the rental.");
        }
        
        if (Status == RentalStatus.Completed)
        {
            return;
        }

        if (Status == RentalStatus.Cancelled)
        {
            throw new InvalidOperationException(
                "A cancelled rental cannot be completed.");
        }

        if (Status != RentalStatus.Approved &&
            Status != RentalStatus.InProgress)
        {
            throw new InvalidOperationException(
                "The rental cannot be completed in its current status.");
        }

        Status = RentalStatus.Completed;
        CompletedByUserId = requesterId;
        CompletedAtUtc = completedAtUtc;
        UpdatedAtUtc = completedAtUtc;
    }
    
    /// <summary>
    /// Cancels the rental and registers the cancellation penalty.
    /// / Cancela o aluguel e registra a multa de cancelamento.
    /// </summary>
    public void Cancel(
        Guid requesterId,
        DateTime cancelledAtUtc)
    {
        const decimal cancellationPenaltyRate = 0.20m;

        if (requesterId == Guid.Empty)
        {
            throw new ArgumentException(
                "Requester id cannot be empty.");
        }

        if (requesterId != ProviderId &&
            requesterId != RenterId)
        {
            throw new InvalidOperationException(
                "Only the rental provider or renter can cancel the rental.");
        }

        // Keeps the operation idempotent.
        // / Mantém a operação idempotente.
        if (Status == RentalStatus.Cancelled)
        {
            return;
        }

        if (Status == RentalStatus.Completed)
        {
            throw new InvalidOperationException(
                "A completed rental cannot be cancelled.");
        }

        if (Status != RentalStatus.Approved &&
            Status != RentalStatus.InProgress)
        {
            throw new InvalidOperationException(
                "The rental cannot be cancelled in its current status.");
        }

        CancellationPenaltyAmount = decimal.Round(
            TotalAmount * cancellationPenaltyRate,
            2,
            MidpointRounding.AwayFromZero);

        Status = RentalStatus.Cancelled;
        CancelledByUserId = requesterId;
        CancelledAtUtc = cancelledAtUtc;
        UpdatedAtUtc = cancelledAtUtc;
    }
}