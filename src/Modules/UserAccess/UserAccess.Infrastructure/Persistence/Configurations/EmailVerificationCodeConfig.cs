using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserAccess.Domain.Entities;

namespace UserAccess.Infrastructure.Persistence.Configurations;

public class EmailVerificationCodeConfig : IEntityTypeConfiguration<EmailVerificationCode>
{
    public void Configure(EntityTypeBuilder<EmailVerificationCode> b)
    {
        b.ToTable("email_verification_codes");
        b.HasKey(x => x.Id);
        
        b.Property(x => x.UserId).IsRequired();
        b.Property(x => x.CodeHash).HasMaxLength(64).IsRequired();
        b.Property(x => x.ExpiresAt).IsRequired();
        b.Property(x => x.ConsumedAt).IsRequired(false);
        b.Property(x => x.Attempts).IsRequired();
        b.Property(x => x.CreatedAt).IsRequired();
        b.Property(x => x.Purpose).HasConversion<int>().IsRequired();
        
        b.HasOne(x => x.User)
            .WithMany()
                .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
        
        b.HasIndex(x => new {x.UserId, x.CreatedAt, x.ExpiresAt });
    }
}