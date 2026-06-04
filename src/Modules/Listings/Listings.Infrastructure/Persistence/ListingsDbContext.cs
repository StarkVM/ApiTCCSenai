using Listings.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Listings.Infrastructure.Persistence;

/// <summary>
/// Database context for the Listings module.
/// / Contexto de banco de dados do módulo de anúncios.
/// </summary>
public sealed class ListingsDbContext : DbContext
{
    public ListingsDbContext(DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<Listing> Listings => Set<Listing>();

    public DbSet<ListingImage> ListingImages => Set<ListingImage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(ListingsDbContext).Assembly);
}