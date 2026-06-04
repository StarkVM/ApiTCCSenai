using Listings.Domain.Interfaces;
using Listings.Infrastructure.Persistence;
using Listings.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Listings.Infrastructure;

/// <summary>
/// Dependency injection configuration for the Listings infrastructure layer.
/// / Configuração de injeção de dependência da camada de infraestrutura de anúncios.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers infrastructure services used by the Listings module.
    /// / Registra os serviços de infraestrutura utilizados pelo módulo de anúncios.
    /// </summary>
    public static IServiceCollection AddListingsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var listingsConnectionString = configuration.GetConnectionString("ListingsDb");

        if (string.IsNullOrWhiteSpace(listingsConnectionString))
        {
            throw new InvalidOperationException("Listings Connection string not found.");
        }

        services.AddDbContext<ListingsDbContext>(options =>
            options.UseNpgsql(listingsConnectionString));
        
        services.AddScoped<IListingRepository, ListingRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}