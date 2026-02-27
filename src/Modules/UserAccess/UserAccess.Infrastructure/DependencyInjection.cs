using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserAccess.Infrastructure.Persistence;

namespace UserAccess.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddUserAccessInfrastructure(this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddDbContext<UserAccessDbContext>(opt =>
            opt.UseNpgsql(configuration.GetConnectionString("UserAccessDb")));
        
        return services;
    }
}