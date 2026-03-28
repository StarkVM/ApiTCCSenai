using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserAccess.Domain.Entities;

namespace UserAccess.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core mapping for refresh token entity.
/// / Mapeamento EF Core da entidade refresh token.
/// </summary>
public class RefreshTokenConfig : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> b)
    {
        b.ToTable("refresh_tokens");
        b.HasKey(x => x.Id);
        
        b.Property(x => x.UserId).IsRequired();
        b.Property(x => x.TokenHash).HasMaxLength(255).IsRequired();
        
        b.HasIndex(x => x.TokenHash).IsUnique();
        
        b.Property(x => x.CreatedAtUtc).IsRequired();
        b.Property(x => x.ExpiresAtUtc).IsRequired();
        
        b.Property(x => x.RevokedAtUtc).IsRequired(false);
        b.Property(x => x.RevokedReason).HasMaxLength(100).IsRequired(false);
        b.Property(x => x.ReplacedByTokenHash).HasMaxLength(255).IsRequired(false);

        b.HasIndex(x => x.UserId);
        b.HasIndex(x => new {x.UserId, x.ExpiresAtUtc});
        
        b.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}