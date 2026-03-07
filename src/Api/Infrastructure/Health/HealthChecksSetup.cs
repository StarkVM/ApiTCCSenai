using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;
using UserAccess.Infrastructure.Persistence;


namespace Api.Infrastructure.Health;

public static class HealthChecksSetup
{
    public static IServiceCollection AddAppHealthChecks(this IServiceCollection services)
    {
        // Registra os checks (DI)
        // Registers the health checks (DI)
        services.AddHealthChecks()
        // Liveness: "o processo está vivo?"
        // Liveness: "is the process alive?"
        .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" })
        
        // Readiness: "dependências críticas estão OK?"
        // Readiness: "are critical dependencies OK?"
        .AddDbContextCheck<UserAccessDbContext>(
            name: "useraccess_db",
            failureStatus: HealthStatus.Unhealthy,
            tags: new[] { "ready" });
        
        return services;
    }
    
    // Mapeia endpoints
    // Maps the endpoints
    public static IEndpointRouteBuilder MapAppHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live"),
        });
        
        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("ready"),
            ResponseWriter = WriteJsonResponse
        });
        
        return endpoints;
    }

    private static Task WriteJsonResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var payload = new
        {
            status = report.Status.ToString(),
            totalDurationMs = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.ToDictionary(
                e => e.Key,
                e => new
                {
                    status = e.Value.Status.ToString(),
                    DurationMs = e.Value.Duration.TotalMilliseconds,
                    error = e.Value.Exception?.Message
                })
        };
        
        return context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
    
}