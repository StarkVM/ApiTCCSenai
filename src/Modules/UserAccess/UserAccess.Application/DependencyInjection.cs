using Microsoft.Extensions.DependencyInjection;
using UserAccess.Application.Auth.Register;
using UserAccess.Application.Auth.ResetPassword;
using UserAccess.Application.Auth.VerificationCodes;
using UserAccess.Domain.Interfaces;

namespace UserAccess.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddUserAccessApplication(this IServiceCollection services)
    {
        services.AddScoped<RegisterUserHandler>();
        
        services.AddScoped<RequestPasswordResetHandler>();
        services.AddScoped<ResetPasswordHandler>();
        
        services.AddScoped<IVerificationCodeService, VerificationCodeService>();
        services.AddScoped<IVerificationCodeSender, VerificationCodeSender>();
        
        return services;
    }
}