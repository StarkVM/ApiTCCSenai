using Listings.Application.CreateListing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Listings.Application;

/// <summary>
/// Dependency injection configuration for the Listings application layer.
/// / Configuração de injeção de dependência da camada de aplicação de anúncios.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers application services used by the Listings module.
    /// / Registra os serviços de aplicação utilizados pelo módulo de anúncios.
    /// </summary>
    public static IServiceCollection AddListingsApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<CreateListingHandler>();
        
        return services;
    }
}