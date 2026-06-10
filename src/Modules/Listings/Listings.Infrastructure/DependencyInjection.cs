using Amazon.Runtime;
using Amazon.S3;
using Listings.Application.Abstractions;
using Listings.Contracts.Listings.Interfaces;
using Listings.Domain.Interfaces;
using Listings.Infrastructure.ModuleCommands;
using Listings.Infrastructure.ModuleQueries;
using Listings.Infrastructure.Persistence;
using Listings.Infrastructure.Persistence.Repositories;
using Listings.Infrastructure.Queries;
using Listings.Infrastructure.Storage.S3;
using Listings.Infrastructure.Time;
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

        services.Configure<S3StorageOptions>(
            configuration.GetSection(S3StorageOptions.SectionName));

        var awsOptions = configuration.GetAWSOptions();

        var awsAccessKey = configuration["AWS:AccessKey"];
        var awsSecretKey = configuration["AWS:SecretKey"];

        if (!string.IsNullOrWhiteSpace(awsAccessKey) &&
            !string.IsNullOrWhiteSpace(awsSecretKey))
        {
            awsOptions.Credentials = new BasicAWSCredentials(
                awsAccessKey,
                awsSecretKey);
        }

        services.AddDefaultAWSOptions(awsOptions);
        services.AddAWSService<IAmazonS3>();

        services.AddScoped<IListingRepository, ListingRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        services.AddScoped<IListingRentalQueries, ListingRentalQueries>();
        services.AddScoped<IListingRentalCommands, ListingRentalCommands>();
        
        services.AddScoped<IListingReadService, ListingReadService>();

        services.AddScoped<IListingImageUrlProvider, S3ListingImageUrlProvider>();

        services.AddScoped<IListingImageStorage, S3ListingImageStorage>();

        services.AddScoped<IClock, SystemClock>();

        return services;
    }
}