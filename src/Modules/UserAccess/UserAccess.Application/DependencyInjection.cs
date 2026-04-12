using Microsoft.Extensions.DependencyInjection;
using UserAccess.Application.Auth.Common.Options;
using UserAccess.Application.Auth.Register;
using UserAccess.Application.Auth.ResetPassword;
using UserAccess.Application.Auth.VerificationCodes;
using UserAccess.Application.Auth.VerifyEmail;
using UserAccess.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using UserAccess.Application.Auth.Common.Services;

namespace UserAccess.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddUserAccessApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<RegisterUserHandler>();
        
        services.AddScoped<RequestPasswordResetHandler>();
        services.AddScoped<ResetPasswordHandler>();

        services.AddScoped<TokenIssuer>();
        
        services.AddScoped<VerifyEmailHandler>();
        services.AddScoped<RequestNewRegisterEmailVerificationCodeHandler>();
        
        services.AddScoped<IVerificationCodeService, VerificationCodeService>();
        services.AddScoped<IVerificationCodeSender, VerificationCodeSender>();
        
        
        services.Configure<RefreshTokenOptions>(configuration.GetSection(RefreshTokenOptions.SectionName));
        
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        
        return services;
    }
}