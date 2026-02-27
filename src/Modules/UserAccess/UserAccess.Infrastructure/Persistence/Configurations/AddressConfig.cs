using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserAccess.Domain.Entities;

namespace UserAccess.Infrastructure.Persistence.Configurations;

public class AddressConfig : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> b)
    {
        b.ToTable("addresses");
        b.HasKey(x => x.UserId);
        
        b.Property(x => x.State).HasMaxLength(50).IsRequired();
        b.Property(x => x.City).HasMaxLength(100).IsRequired();
        b.Property(x => x.District).HasMaxLength(100).IsRequired();
        b.Property(x => x.Street).HasMaxLength(200).IsRequired();
        b.Property(x => x.ZipCode).HasMaxLength(20).IsRequired();
        
        b.Property(x => x.CreatedAt).IsRequired();
        b.Property(x => x.UpdatedAt).IsRequired();
    }
}