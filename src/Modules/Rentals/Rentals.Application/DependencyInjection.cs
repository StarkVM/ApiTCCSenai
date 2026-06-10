using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rentals.Application.CompleteRental;
using Rentals.Application.CreateRental;
using Rentals.Application.GetRentals;

namespace Rentals.Application;

/// <summary>
/// Dependency injection configuration for the Rentals application layer.
/// / Configuração de injeção de dependência da camada de aplicação de aluguéis.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers application services used by the Rentals module.
    /// / Registra os serviços de aplicação utilizados pelo módulo de aluguéis.
    /// </summary>
    public static IServiceCollection AddRentalsApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<CreateRentalHandler>();
        services.AddScoped<CompleteRentalHandler>();
        services.AddScoped<GetRentalsHandler>();

        return services;
    }
}