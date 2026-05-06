using Microsoft.EntityFrameworkCore;
using UserAccess.Domain.Entities;

namespace UserAccess.Infrastructure.Persistence;

public class UserAccessDbContext : DbContext
{ 
     public UserAccessDbContext(DbContextOptions<UserAccessDbContext> options) : base(options) { }
    
     public  DbSet<User> Users =>  Set<User>();
     public DbSet<Address> Addresses =>  Set<Address>();
     public DbSet<EmailVerificationCode> EmailVerificationCodes =>  Set<EmailVerificationCode>();
     
     public DbSet<RefreshToken> RefreshTokens =>  Set<RefreshToken>();
     
     public DbSet<IdentityVerificationSession> IdentityVerificationSessions =>  Set<IdentityVerificationSession>();

     protected override void OnModelCreating(ModelBuilder modelBuilder) =>
         modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserAccessDbContext).Assembly);
}