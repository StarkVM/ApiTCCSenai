using Microsoft.Extensions.DependencyInjection;
using UserAccess.Application.Auth.Register;
using UserAccess.Application.Auth.ResetPassword;
using UserAccess.Application.Auth.VerificationCodes;
using UserAccess.Application.Auth.VerifyEmail;
using UserAccess.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using UserAccess.Application.Auth.Common.Services;
using UserAccess.Application.Auth.Login;

namespace UserAccess.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddUserAccessApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<RegisterUserHandler>();
        services.AddScoped<VerifyEmailHandler>();
        services.AddScoped<RequestNewRegisterEmailVerificationCodeHandler>();
        
        services.AddScoped<LoginHandler>();
        services.AddScoped<RequestNewLoginVerificationCodeHandler>();
        services.AddScoped<VerifyLoginHandler>();
        
        services.AddScoped<RequestPasswordResetHandler>();
        services.AddScoped<ResetPasswordHandler>();

        services.AddScoped<TokenIssuer>();
        
        services.AddScoped<IVerificationCodeService, VerificationCodeService>();
        services.AddScoped<IVerificationCodeSender, VerificationCodeSender>();
        
        return services;
    }
}