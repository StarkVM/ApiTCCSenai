using Api.Infrastructure.Health;
using Api.Routes.UserAccess;
using Api.Infrastructure.RateLimiting;
using UserAccess.Infrastructure;
using Api.Infrastructure.Logging;
using Serilog;

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
        builder.Services.AddSwaggerGen();
        
        // Módulos (cada módulo registra sua própria infra)
        // Modules (each module registers its own infrastructure)
        builder.Services.AddUserAccessInfrastructure(builder.Configuration);
        
        // Health checks (infra do host)
        // Health checks (host infrastructure)
        builder.Services.AddHealthChecks();
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

        app.UseHttpsRedirection();
        
        // Adiciona middleware que registra cada requisição HTTP em um único log resumido (método, rota, status e tempo de execução).
        // Adds middleware that logs each HTTP request as a single summarized log entry (method, route, status code, and execution time).
        app.UseSerilogRequestLogging();

        app.UseRateLimiter();

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
        
        return app;
    }
}