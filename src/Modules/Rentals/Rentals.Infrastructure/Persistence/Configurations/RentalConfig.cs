using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rentals.Domain.Entities;

namespace Rentals.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core mapping configuration for the Rental entity.
/// / Configuração de mapeamento EF Core para a entidade Rental.
/// </summary>
public sealed class RentalConfig : IEntityTypeConfiguration<Rental>
{
    public void Configure(EntityTypeBuilder<Rental> b)
    {
        b.ToTable("rentals", table =>
        {
            table.HasCheckConstraint(
                "ck_rentals_owner_and_renter_must_be_different",
                "provider_id <> renter_id");

            table.HasCheckConstraint(
                "ck_rentals_total_days_positive",
                "total_days > 0");

            table.HasCheckConstraint(
                "ck_rentals_listing_daily_price_positive",
                "listing_daily_price_snapshot > 0");

            table.HasCheckConstraint(
                "ck_rentals_operator_prices_non_negative",
                "operator_daily_price_snapshot >= 0 AND operator_subtotal >= 0");

            table.HasCheckConstraint(
                "ck_rentals_freight_prices_non_negative",
                "freight_fixed_price_snapshot >= 0 AND freight_subtotal >= 0");

            table.HasCheckConstraint(
                "ck_rentals_amounts_non_negative",
                "machine_subtotal > 0 AND total_amount > 0");

            table.HasCheckConstraint(
                "ck_rentals_total_amount_consistency",
                "total_amount = machine_subtotal + operator_subtotal + freight_subtotal");
            
            table.HasCheckConstraint(
                "ck_rentals_cancellation_penalty_non_negative",
                "cancellation_penalty_amount >= 0");
        });

        b.HasKey(x => x.Id);

        b.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever()
            .IsRequired();

        b.Property(x => x.ListingId)
            .HasColumnName("listing_id")
            .IsRequired();

        b.Property(x => x.ProviderId)
            .HasColumnName("provider_id")
            .IsRequired();

        b.Property(x => x.RenterId)
            .HasColumnName("renter_id")
            .IsRequired();

        b.Property(x => x.StartDate)
            .HasColumnName("start_date")
            .HasColumnType("date")
            .IsRequired();

        b.Property(x => x.EndDate)
            .HasColumnName("end_date")
            .HasColumnType("date")
            .IsRequired();

        b.Property(x => x.TotalDays)
            .HasColumnName("total_days")
            .IsRequired();

        b.Property(x => x.IncludeOperator)
            .HasColumnName("include_operator")
            .IsRequired();

        b.Property(x => x.IncludeFreight)
            .HasColumnName("include_freight")
            .IsRequired();

        b.Property(x => x.ListingDailyPriceSnapshot)
            .HasColumnName("listing_daily_price_snapshot")
            .HasPrecision(18, 2)
            .IsRequired();

        b.Property(x => x.OperatorDailyPriceSnapshot)
            .HasColumnName("operator_daily_price_snapshot")
            .HasPrecision(18, 2)
            .IsRequired();

        b.Property(x => x.FreightFixedPriceSnapshot)
            .HasColumnName("freight_fixed_price_snapshot")
            .HasPrecision(18, 2)
            .IsRequired();

        b.Property(x => x.MachineSubtotal)
            .HasColumnName("machine_subtotal")
            .HasPrecision(18, 2)
            .IsRequired();

        b.Property(x => x.OperatorSubtotal)
            .HasColumnName("operator_subtotal")
            .HasPrecision(18, 2)
            .IsRequired();

        b.Property(x => x.FreightSubtotal)
            .HasColumnName("freight_subtotal")
            .HasPrecision(18, 2)
            .IsRequired();

        b.Property(x => x.TotalAmount)
            .HasColumnName("total_amount")
            .HasPrecision(18, 2)
            .IsRequired();

        b.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        b.Property(x => x.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        b.Property(x => x.ApprovedAtUtc)
            .HasColumnName("approved_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        b.Property(x => x.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        b.Property(x => x.StartedAtUtc)
            .HasColumnName("started_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired(false);

        b.Property(x => x.CompletedAtUtc)
            .HasColumnName("completed_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired(false);
        
        b.Property(x => x.CompletedByUserId)
            .HasColumnName("completed_by_user_id")
            .IsRequired(false);
        
        b.Property(x => x.CancelledByUserId)
            .HasColumnName("cancelled_by_user_id")
            .IsRequired(false);

        b.Property(x => x.CancellationPenaltyAmount)
            .HasColumnName("cancellation_penalty_amount")
            .HasPrecision(18, 2)
            .HasDefaultValue(0m)
            .IsRequired();

        b.Property(x => x.CancelledAtUtc)
            .HasColumnName("cancelled_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired(false);

        b.HasIndex(x => x.ListingId)
            .HasDatabaseName("ix_rentals_listing_id");

        b.HasIndex(x => x.ProviderId)
            .HasDatabaseName("ix_rentals_provider_id");

        b.HasIndex(x => x.RenterId)
            .HasDatabaseName("ix_rentals_renter_id");

        b.HasIndex(x => x.Status)
            .HasDatabaseName("ix_rentals_status");

        b.HasIndex(x => new { x.ListingId, x.Status })
            .HasDatabaseName("ix_rentals_listing_id_status");

        b.HasIndex(x => new { x.RenterId, x.Status })
            .HasDatabaseName("ix_rentals_renter_id_status");

        b.HasIndex(x => new { x.ProviderId, x.Status })
            .HasDatabaseName("ix_rentals_provider_id_status");

        b.HasIndex(x => new { x.StartDate, x.EndDate })
            .HasDatabaseName("ix_rentals_start_date_end_date");
        
        b.HasIndex(x => x.CompletedByUserId)
            .HasDatabaseName("ix_rentals_completed_by_user_id");
        
        b.HasIndex(x => x.CancelledByUserId)
            .HasDatabaseName("ix_rentals_cancelled_by_user_id");
    }
}