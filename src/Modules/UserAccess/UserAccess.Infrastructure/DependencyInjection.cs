using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserAccess.Domain.Interfaces;
using UserAccess.Infrastructure.Email;
using UserAccess.Infrastructure.Persistence;
using UserAccess.Infrastructure.Persistence.Repositories;
using UserAccess.Infrastructure.Security;
using UserAccess.Infrastructure.Time;

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
        
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IVerificationCodeRepository, VerificationCodeRepository>();
        services.AddScoped<IClock, SystemClock>();
        services.AddScoped<IEmailSender, EmailSenderFake>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
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

        services.AddSingleton<ICpfHasher>(_ => new CpfHasher(cpfSecretKey));
        services.AddSingleton<IVerificationCodeHasher>(_ => new VerificationCodeHasher(codeSecretKey));
        
        return services;
    }
}