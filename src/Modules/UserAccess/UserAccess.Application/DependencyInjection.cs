using Microsoft.Extensions.DependencyInjection;
using UserAccess.Application.Auth.Register;
using UserAccess.Application.Auth.ResetPassword;
using UserAccess.Application.Auth.VerificationCodes;
using UserAccess.Application.Auth.VerifyEmail;
using UserAccess.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using UserAccess.Application.Auth.Tokens;
using UserAccess.Application.Auth.Login;
using UserAccess.Application.Auth.RefreshTokens;
using UserAccess.Application.CurrentUser.Me;

namespace UserAccess.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddUserAccessApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<RefreshTokensHandler>();
        
        services.AddScoped<RegisterUserHandler>();
        services.AddScoped<VerifyEmailHandler>();
        services.AddScoped<RequestNewRegisterEmailVerificationCodeHandler>();
        
        services.AddScoped<LoginHandler>();
        services.AddScoped<RequestNewLoginVerificationCodeHandler>();
        services.AddScoped<VerifyLoginHandler>();
        
        services.AddScoped<RequestPasswordResetHandler>();
        services.AddScoped<ResetPasswordHandler>();

        services.AddScoped<ITokenIssuer, TokenIssuer>();
        
        services.AddScoped<IVerificationCodeService, VerificationCodeService>();
        services.AddScoped<IVerificationCodeSender, VerificationCodeSender>();
        
        services.AddScoped<MeHandler>();
        
        return services;
    }
}