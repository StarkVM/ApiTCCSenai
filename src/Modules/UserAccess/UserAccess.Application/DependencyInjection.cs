using Microsoft.Extensions.DependencyInjection;
using UserAccess.Application.Auth.Register;

namespace UserAccess.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddUserAccessApplication(this IServiceCollection services)
    {
        services.AddScoped<RegisterUserHandler>();
        
        return services;
    }
}