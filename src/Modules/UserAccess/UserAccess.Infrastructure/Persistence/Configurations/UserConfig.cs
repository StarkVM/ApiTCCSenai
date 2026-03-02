using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserAccess.Domain.Entities;

namespace UserAccess.Infrastructure.Persistence.Configurations;

public class UserConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("users");
        b.HasKey(x => x.Id);
        
        b.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
        b.Property(x => x.LastName).HasMaxLength(100).IsRequired();
        b.Property(x => x.BirthDate).IsRequired();
        
        b.Property(x => x.Email).HasMaxLength(255).IsRequired();
        b.HasIndex(x => x.Email).IsUnique();
        
        b.Property(x => x.CpfHash).HasMaxLength(64).IsRequired();
        b.HasIndex(x => x.CpfHash).IsUnique();
        
        b.Property(x => x.PasswordHash).HasMaxLength(255).IsRequired();
        b.Property(x => x.PasswordChangedAt).IsRequired();
        b.Property(x => x.EmailVerifiedAt);
        b.Property(x => x.CreatedAt).IsRequired();
        
        b.HasOne(x => x.Address)
            .WithOne(x => x.User)
                .HasForeignKey<Address>(x => x.UserId);
    }
}