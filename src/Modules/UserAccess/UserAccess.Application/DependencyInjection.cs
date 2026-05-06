using Microsoft.Extensions.DependencyInjection;
using UserAccess.Application.Auth.Register;
using UserAccess.Application.Auth.ResetPassword;
using UserAccess.Application.Auth.Services.VerificationCodesServices;
using UserAccess.Application.Auth.VerifyEmail;
using UserAccess.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using UserAccess.Application.Auth.Services.TokensServices;
using UserAccess.Application.Auth.Login;
using UserAccess.Application.Auth.Logout;
using UserAccess.Application.Auth.RefreshTokens;
using UserAccess.Application.CurrentUser.Me;
using UserAccess.Application.IdentityVerification.CreateIdentityVerificationSession;

namespace UserAccess.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddUserAccessApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<RefreshTokensHandler>();
        
        services.AddScoped<CreateIdentityVerificationSessionHandler>();
        
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
        
        services.AddScoped<LogoutCurrentSessionHandler>();
        services.AddScoped<LogoutAllSessionsHandler>();
        
        services.AddScoped<MeHandler>();
        
        return services;
    }
}