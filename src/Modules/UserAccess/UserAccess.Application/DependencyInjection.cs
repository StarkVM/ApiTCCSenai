using Microsoft.Extensions.DependencyInjection;
using UserAccess.Application.Auth.Register;
using UserAccess.Application.Auth.VerifyEmail;

namespace UserAccess.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddUserAccessApplication(this IServiceCollection services)
    {
        services.AddScoped<RegisterUserHandler>();
        services.AddScoped<SendEmailVerificationCode>();
        
        return services;
    }
}