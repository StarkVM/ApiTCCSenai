using Microsoft.EntityFrameworkCore;
using Rentals.Domain.Entities;

namespace Rentals.Infrastructure.Persistence;

public sealed class RentalsDbContext : DbContext
{
    public RentalsDbContext(
        DbContextOptions<RentalsDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<Rental> Rentals => Set<Rental>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(RentalsDbContext).Assembly);
    }
}