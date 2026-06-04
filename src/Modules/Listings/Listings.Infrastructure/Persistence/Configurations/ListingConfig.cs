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

        b.HasMany(x => x.Images)
            .WithOne(x => x.Listing)
            .HasForeignKey(x => x.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Navigation(x => x.Images)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}