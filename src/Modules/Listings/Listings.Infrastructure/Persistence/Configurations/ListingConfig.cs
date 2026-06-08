using Listings.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Listings.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core mapping for the listing entity.
/// / Mapeamento EF Core da entidade anúncio.
/// </summary>
public sealed class ListingConfig : IEntityTypeConfiguration<Listing>
{
    public void Configure(EntityTypeBuilder<Listing> b)
    {
        b.ToTable("listings");

        b.HasKey(x => x.Id);

        b.Property(x => x.OwnerId)
            .IsRequired();

        b.HasIndex(x => x.OwnerId);

        b.Property(x => x.Title)
            .HasMaxLength(150)
            .IsRequired();

        b.Property(x => x.Description)
            .HasMaxLength(2000)
            .IsRequired();

        b.Property(x => x.Category)
            .HasConversion<int>()
            .IsRequired();

        b.HasIndex(x => x.Category);

        b.Property(x => x.DailyPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        b.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        b.HasIndex(x => x.Status);

        b.Property(x => x.CreatedAtUtc)
            .IsRequired();

        b.Property(x => x.UpdatedAtUtc)
            .IsRequired();

        b.Property(x => x.ReviewedAtUtc)
            .IsRequired(false);

        b.Property(x => x.RejectionReason)
            .HasMaxLength(500)
            .IsRequired(false);
        
        b.Property(x => x.IsFleet)
            .IsRequired();

        b.OwnsOne(x => x.PickupAddress, pickupAddress =>
        {
            pickupAddress.Property(x => x.State)
                .HasColumnName("pickup_state")
                .HasMaxLength(100)
                .IsRequired();

            pickupAddress.Property(x => x.City)
                .HasColumnName("pickup_city")
                .HasMaxLength(100)
                .IsRequired();

            pickupAddress.Property(x => x.District)
                .HasColumnName("pickup_district")
                .HasMaxLength(100)
                .IsRequired();

            pickupAddress.Property(x => x.Street)
                .HasColumnName("pickup_street")
                .HasMaxLength(150)
                .IsRequired();

            pickupAddress.Property(x => x.Number)
                .HasColumnName("pickup_number")
                .HasMaxLength(20)
                .IsRequired();

            pickupAddress.Property(x => x.ZipCode)
                .HasColumnName("pickup_zip_code")
                .HasMaxLength(8)
                .IsRequired();

            pickupAddress.Property(x => x.Complement)
                .HasColumnName("pickup_complement")
                .HasMaxLength(150)
                .IsRequired(false);
        });

        b.Navigation(x => x.PickupAddress)
            .IsRequired();

        b.OwnsOne(x => x.OperatorOption, operatorOption =>
        {
            operatorOption.Property(x => x.IsAvailable)
                .HasColumnName("operator_available")
                .IsRequired();

            operatorOption.Property(x => x.AdditionalDailyPrice)
                .HasColumnName("operator_daily_price")
                .HasPrecision(18, 2)
                .IsRequired();
        });

        b.Navigation(x => x.OperatorOption)
            .IsRequired();

        b.OwnsOne(x => x.FreightOption, freightOption =>
        {
            freightOption.Property(x => x.IsAvailable)
                .HasColumnName("freight_available")
                .IsRequired();

            freightOption.Property(x => x.FixedPrice)
                .HasColumnName("freight_fixed_price")
                .HasPrecision(18, 2)
                .IsRequired();
        });

        b.Navigation(x => x.FreightOption)
            .IsRequired();

        b.HasMany(x => x.Images)
            .WithOne(x => x.Listing)
            .HasForeignKey(x => x.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Navigation(x => x.Images)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}