using Api.Infrastructure.Health;
using UserAccess.Infrastructure;

namespace Api.Configuration;

public static class ConfigureServices
{
    // 1) Tudo que é "registrar serviços" (DI) fica aqui
    // 1) Everything related to "service registration" (DI) goes here
    public static WebApplicationBuilder AddApiServices(this WebApplicationBuilder builder)
    {
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

        // Host-level endpoints (host infrastructure)
        app.MapAppHealthChecks();
        
        // Endpoints por módulo (quando implementar):
        // Module endpoints (when you implement them):
        // app.MapUserAccessRoutes();
        
        return app;
    }
}