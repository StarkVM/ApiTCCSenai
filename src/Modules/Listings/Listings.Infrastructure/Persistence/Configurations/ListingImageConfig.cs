using Listings.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Listings.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core mapping for the listing image entity.
/// / Mapeamento EF Core da entidade imagem de anúncio.
/// </summary>
public sealed class ListingImageConfig : IEntityTypeConfiguration<ListingImage>
{
    public void Configure(EntityTypeBuilder<ListingImage> b)
    {
        b.ToTable("listing_images");

        b.HasKey(x => x.Id);

        b.Property(x => x.ListingId)
            .IsRequired();

        b.HasIndex(x => x.ListingId);

        b.Property(x => x.StorageKey)
            .HasMaxLength(500)
            .IsRequired();

        b.HasIndex(x => x.StorageKey)
            .IsUnique();

        b.Property(x => x.DisplayOrder)
            .IsRequired();

        b.HasIndex(x => new { x.ListingId, x.DisplayOrder })
            .IsUnique();

        b.Property(x => x.CreatedAtUtc)
            .IsRequired();

    }
}