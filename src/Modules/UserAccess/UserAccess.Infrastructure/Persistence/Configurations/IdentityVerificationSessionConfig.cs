using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserAccess.Domain.Entities;

namespace UserAccess.Infrastructure.Persistence.Configurations;

public class IdentityVerificationSessionConfig : IEntityTypeConfiguration<IdentityVerificationSession>
{
    public void Configure(EntityTypeBuilder<IdentityVerificationSession> b)
    {
        b.ToTable("identity_verification_sessions");

        b.HasKey(x => x.Id);
        
        //b.Property(x => x.Id).ValueGeneratedNever();

        b.Property(x => x.UserId).IsRequired();
            b.HasIndex(x => x.UserId);
            //.HasDatabaseName("ix_identity_verification_sessions_user_id");

        b.Property(x => x.ProviderSessionId).IsRequired().HasMaxLength(150);
            b.HasIndex(x => x.ProviderSessionId).IsUnique();
            //.HasDatabaseName("ux_identity_verification_sessions_session_id");

        b.Property(x => x.ProviderSessionUrl).IsRequired().HasMaxLength(1000);

        b.Property(x => x.Status).IsRequired().HasConversion<int>();
        
        //HasConversion<string>().HasMaxLength(50);
        
        b.Property(x => x.Provider).IsRequired().HasConversion<int>();

        b.Property(x => x.CreatedAtUtc).IsRequired();

        b.Property(x => x.CompletedAtUtc).IsRequired(false);

           

            b.HasIndex(x => new { x.UserId, x.Status });
            //.HasDatabaseName("ix_identity_verification_sessions_user_id_status");
            
        
            b.HasIndex(x => new { x.Provider, x.ProviderSessionId }).IsUnique();

        b.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}