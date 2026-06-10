using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rentals.Application.Abstractions;
using Rentals.Domain.Interfaces;
using Rentals.Infrastructure.Persistence;
using Rentals.Infrastructure.Persistence.Repositories;
using Rentals.Infrastructure.Queries;
using Rentals.Infrastructure.Time;

namespace Rentals.Infrastructure;

/// <summary>
/// Dependency injection configuration for the Rentals infrastructure layer.
/// / Configuração de injeção de dependência da camada de infraestrutura de aluguéis.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers infrastructure services used by the Rentals module.
    /// / Registra os serviços de infraestrutura utilizados pelo módulo de aluguéis.
    /// </summary>
    public static IServiceCollection AddRentalsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var rentalsConnectionString =
            configuration.GetConnectionString("RentalsDb");

        if (string.IsNullOrWhiteSpace(rentalsConnectionString))
        {
            throw new InvalidOperationException(
                "Rentals connection string not found.");
        }

        services.AddDbContext<RentalsDbContext>(options =>
            options.UseNpgsql(rentalsConnectionString));

        services.AddScoped<IRentalRepository, RentalRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IClock, SystemClock>();
        services.AddScoped<IRentalReadService, RentalReadService>();

        return services;
    }
}