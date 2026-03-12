using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserAccess.Domain.Interfaces;
using UserAccess.Infrastructure.Persistence;
using UserAccess.Infrastructure.Security;
namespace UserAccess.Infrastructure;


public static class DependencyInjection
{
    public static IServiceCollection AddUserAccessInfrastructure(this IServiceCollection services, 
        IConfiguration configuration)
    {
        var userAccessConnectionString = configuration.GetConnectionString("UserAccessDb");

        if (string.IsNullOrWhiteSpace(userAccessConnectionString))
        {
            throw new InvalidOperationException("User Access Connection string not found");
        }
        
        services.AddDbContext<UserAccessDbContext>(opt =>
            opt.UseNpgsql(userAccessConnectionString));
        
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        
        var cpfSecretKey = configuration["Security:CpfProtectionKey"];

        if (string.IsNullOrWhiteSpace(cpfSecretKey))
        {
            throw new InvalidOperationException("CPF Secret Key not configured");
        }

        services.AddSingleton<ICpfHasher>(_ => new CpfHasher(cpfSecretKey));
        
        return services;
    }
}