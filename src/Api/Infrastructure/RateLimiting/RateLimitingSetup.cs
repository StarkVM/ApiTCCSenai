using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Infrastructure.RateLimiting;

public static class RateLimitingSetup
{
    // Português
    // Registra as políticas de rate limiting da API
    // English
    // Registers the API rate limiting policies
    public static IServiceCollection AddAppRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // Português
            // Resposta padrão quando o limite for excedido
            // English
            // Default response when the limit is exceeded
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            
            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.ContentType = "application/json; charset=utf-8";

                await context.HttpContext.Response.WriteAsync(
                    """
                    {
                        "error": "too_many_requests",
                        "message": "Too Many Requests. Please try again later"
                    }
                    """,
                    cancellationToken
                );
            };

            // Português
            // Política para endpoints públicos gerais
            // English
            // Policy for general public endpoints
            options.AddPolicy("public", httpContext =>
               RateLimitPartition.GetFixedWindowLimiter(
                     partitionKey: GetPartitionKey(httpContext),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 30,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));
            // Português
            // Política mais rígida para endpoints sensíveis de autenticação
            // English
            // Stricter policy for sensitive authentication endpoints
            options.AddPolicy("auth", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetPartitionKey(httpContext),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));
            // Português
            // Política opcional para bursts curtos sem permitir spam contínuo
            // English
            // Optional policy for short bursts without allowing continuous spam
            options.AddPolicy("burst", httpContext =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: GetPartitionKey(httpContext),
                    factory: _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(30),
                        SegmentsPerWindow = 3,
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));
        });

        return services;
    }
    
    // Português
    // Gera a chave usada para separar os limites por IdUser ou IP
    // English
    // Generates the key used to separate limits by IdUser or IP
    private static string GetPartitionKey(HttpContext httpContext)
    {
        
        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)?? 
                     httpContext.User.FindFirstValue("sub");
        
        if (!string.IsNullOrEmpty(userId))
        {
            return $"user: {userId}";
        }
        
        var email = httpContext.User.FindFirstValue(ClaimTypes.Email)??
                    httpContext.User.FindFirstValue("email");

        if (!string.IsNullOrEmpty(email))
        {
            return $"email: {email}";
        }
        
        var ip = httpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
        
        if (!string.IsNullOrEmpty(ip))
        {
            return $"ip: {ip}";
        }

        return "anonymous";
    }
}