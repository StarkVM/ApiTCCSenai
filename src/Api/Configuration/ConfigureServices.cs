using Api.Infrastructure.Health;
using Api.Routes.UserAccess;
using Api.Infrastructure.RateLimiting;
using UserAccess.Infrastructure;
using Api.Infrastructure.Logging;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using Api.Configuration.Specific;
using Api.Routes.Listings;
using UserAccess.Application;
using Listings.Application;
using Listings.Infrastructure;

namespace Api.Configuration;

public static class ConfigureServices
{
    // 1) Tudo que é "registrar serviços" (DI) fica aqui
    // 1) Everything related to "service registration" (DI) goes here
    public static WebApplicationBuilder AddApiServices(this WebApplicationBuilder builder)
    {
        // Configura o pipeline de logging da aplicação centralizando Serilog e suas configurações
        // Configures the application's logging pipeline centralizing Serilog and its settings
        builder.AddAppLogging();
        
        // Swagger / OpenAPI (infra do host)
        // Swagger / OpenAPI (host infrastructure)
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerConfiguration();
        
        // Português
        // Define quais headers encaminhados por proxies confiáveis devem ser processados pelo ASP.NET Core.
        // X-Forwarded-For: identifica o IP real do cliente.
        // X-Forwarded-Proto: indica o protocolo original da requisição (HTTP ou HTTPS).
        //
        // English
        // Specifies which forwarded headers from trusted proxies should be processed by ASP.NET Core.
        // X-Forwarded-For: identifies the real client IP.
        // X-Forwarded-Proto: indicates the original request protocol (HTTP or HTTPS).
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor |
                    ForwardedHeaders.XForwardedProto;
            });
        
        // Módulos (cada módulo registra sua própria infra)
        // Modules (each module registers its own infrastructure)
        builder.Services.AddUserAccessInfrastructure(builder.Configuration);
        builder.Services.AddUserAccessApplication(builder.Configuration);
        
        builder.Services.AddListingsInfrastructure(builder.Configuration);
        builder.Services.AddListingsApplication(builder.Configuration);
        
        //Add the Access Token Settings
        builder.AddAccessTokenConfiguration();
        
        builder.Services.AddAuthorization();
        
        // Health checks (infra do host)
        // Health checks (host infrastructure)
        builder.Services.AddAppHealthChecks();
        
        
        builder.Services.AddAppRateLimiting();

        return builder;
    }

    // 2) Tudo que é "montar pipeline + endpoints do host" fica aqui
    // 2) Everything related to "building the pipeline + host endpoints" goes here
    public static WebApplication UseApi(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        
        // Português
        // Habilita processamento de headers encaminhados antes de middlewares que dependem do IP real
        // English
        // Enables processing of forwarded headers before middlewares that depend on the real client IP
        
        app.UseForwardedHeaders();

        app.UseHttpsRedirection();
        
        // Português:
        // Gera ou reaproveita um RequestId e o injeta no contexto da requisição.
        // English:
        // Generates or reuses a RequestId and injects it into the request context.
        app.UseRequestId();
        
        // Português:
        // Registra cada requisição HTTP em um log resumido.
        // English:
        // Logs each HTTP request as a summarized log entry.
        app.UseSerilogRequestLogging(options =>
        {
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestId", httpContext.TraceIdentifier);
                diagnosticContext.Set("RequestMethod", httpContext.Request.Method);
                diagnosticContext.Set("RequestPath", httpContext.Request.Path);
                diagnosticContext.Set("QueryString", httpContext.Request.QueryString.Value);
                diagnosticContext.Set("ClientIp", httpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString());
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
            };
        } );

        app.UseRateLimiter();

        app.UseAuthentication();
        app.UseAuthorization();
        
        // Host-level endpoints (host infrastructure)
        app.MapAppHealthChecks();
        
        // Português
        // Grupo raiz versionado (padrão para todos os módulos)
        // English
        // Versioned root group (standard for all modules)
        var v1 = app.MapGroup("/api/v1");
        
        // Endpoints por módulo (quando implementar):
        // Module endpoints (when you implement them):
        // app.MapUserAccessRoutes();
        v1.MapUserAccessRoutes();
        v1.MapListingsRoutes();
        
        return app;
    }
}