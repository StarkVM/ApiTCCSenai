using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using UserAccess.Domain.Interfaces;
using UserAccess.Infrastructure.Email;
using UserAccess.Infrastructure.Persistence;
using UserAccess.Infrastructure.Persistence.Repositories;
using UserAccess.Infrastructure.Security;
using UserAccess.Infrastructure.Time;
using UserAccess.Infrastructure.Auth.Options;
using Resend;
using UserAccess.Application.Abstractions;
using UserAccess.Contracts.Users.Interfaces;
using UserAccess.Infrastructure.Auth.Generators;
using UserAccess.Infrastructure.CpfIdentityVerification;
using UserAccess.Infrastructure.CpfIdentityVerification.Options;
using UserAccess.Infrastructure.IdentityVerification.Didit;
using UserAccess.Infrastructure.IdentityVerification.Didit.Options;
using UserAccess.Infrastructure.IdentityVerification.Didit.Payloads;
using UserAccess.Infrastructure.IdentityVerification.Didit.SignatureValidator;
using UserAccess.Infrastructure.ModuleQueries;
using UserAccess.Infrastructure.Storage;

namespace UserAccess.Infrastructure;


public static class DependencyInjection
{
    public static IServiceCollection AddUserAccessInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var userAccessConnectionString = configuration.GetConnectionString("UserAccessDb");

        if (string.IsNullOrWhiteSpace(userAccessConnectionString))
        {
            throw new InvalidOperationException("User Access Connection string not found");
        }
        
        services.AddDbContext<UserAccessDbContext>(opt =>
            opt.UseNpgsql(userAccessConnectionString));

        services.Configure<UserProfilePhotoStorageOptions>(
            configuration.GetSection("UserAccess:ProfilePhotos:S3"));

        services.AddScoped<IUserProfilePhotoStorage, S3UserProfilePhotoStorage>();
        services.AddScoped<IUserProfilePhotoUrlProvider, S3UserProfilePhotoUrlProvider>();
        
        services.AddScoped<IUserAccessQueries, UserAccessQueries>();
        
        services.Configure<RefreshTokenOptions>(configuration.GetSection(RefreshTokenOptions.SectionName));
        
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        
        services.AddScoped<IRefreshTokenLifetimeProvider, RefreshTokenLifetimeProvider>();
        services.AddScoped<IAccessTokenLifetimeProvider, AccessTokenLifetimeProvider>();
        services.AddSingleton<IRefreshTokenGenerator, RefreshTokenGenerator>();
        services.AddScoped<IAccessTokenGenerator, AccessTokenGenerator>();
        services.AddSingleton<IRefreshTokenHasher, RefreshTokenHasher>();
        
        services.AddScoped<IUserPublicProfileQueries, UserPublicProfileQueries>();
        
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IVerificationCodeRepository, VerificationCodeRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IIdentityVerificationRepository, IdentityVerificationRepository>();
        
        services.AddScoped<IClock, SystemClock>();
        
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        services.Configure<ApiCpfOptions>(
            configuration.GetSection("ApiCpf"));

        services.AddHttpClient<ICpfValidator, CpfValidator>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<ApiCpfOptions>>().Value;

            if (string.IsNullOrWhiteSpace(options.BaseUrl))
            {
                throw new InvalidOperationException("ApiCpf BaseUrl is not configured.");
            }

            if (string.IsNullOrWhiteSpace(options.ApiKey))
            {
                throw new InvalidOperationException("ApiCpf ApiKey is not configured.");
            }

            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(10);

            client.DefaultRequestHeaders.Add("X-API-KEY", options.ApiKey);
        });
        
        services.Configure<DiditOptions>(
            configuration.GetSection("Didit"));

        services.AddHttpClient<IIdentityVerificationProvider,DiditClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<DiditOptions>>().Value;

            if (string.IsNullOrWhiteSpace(options.BaseUrl))
                throw new InvalidOperationException("Didit BaseUrl is not configured.");

            if (string.IsNullOrWhiteSpace(options.ApiKey))
                throw new InvalidOperationException("Didit ApiKey is not configured.");

            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(10);

            client.DefaultRequestHeaders.Add("X-API-KEY", options.ApiKey);
        });
        
        services.AddScoped<IIdentityVerificationWebhookParser, IdentityVerificationWebhookParser>();
        services.AddScoped<IIdentityVerificationWebhookAuthenticator, DiditWebhookSignatureValidator>();
        
        var cpfSecretKey = configuration["Security:CpfProtectionKey"];

        if (string.IsNullOrWhiteSpace(cpfSecretKey))
        {
            throw new InvalidOperationException("CPF Secret Key not configured");
        }
        
        var codeSecretKey = configuration["Security:CodeProtectionKey"];

        if (string.IsNullOrWhiteSpace(codeSecretKey))
        {
            throw new InvalidOperationException("Code Secret Key not configured");
        }

        services.AddOptions();

        services.AddHttpClient<ResendClient>();

        services.Configure<ResendClientOptions>(options =>
        {
            options.ApiToken = configuration["Email:ApiKey"]
                               ?? throw new InvalidOperationException("Resend API key is not configured.");
        });

        services.AddTransient<IResend, ResendClient>();

        services.AddScoped<IEmailSender>(sp =>
        {
            var resend = sp.GetRequiredService<IResend>();
            
            var fromEmail = configuration["Email:From"]
                            ?? throw new InvalidOperationException("Sender email is not configured.");

            return new EmailSender(resend, fromEmail);
        });
        
        services.AddSingleton<ICpfHasher>(_ => new CpfHasher(cpfSecretKey));
        services.AddSingleton<IVerificationCodeHasher>(_ => new VerificationCodeHasher(codeSecretKey));
        
        return services;
    }
}