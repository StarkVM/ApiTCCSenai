using Serilog;

namespace Api.Infrastructure.Logging;

public static class LoggingSetup
{
    public static WebApplicationBuilder AddAppLogging(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, configuration) =>
        {

            configuration
                // Português
                // Lê configurações do appsettings.json
                // English
                // Reads settings from appsettings.json
                .ReadFrom.Configuration(context.Configuration)

                // Português
                // Permite que o Serilog use serviços do container de DI
                // English
                // Allows Serilog to use services from the DI container
                .ReadFrom.Services(services)
                // Português
                // Adiciona propriedades do contexto atual aos logs
                // English
                // Adds current context properties to the logs
                .Enrich.FromLogContext();
        });
        
        return builder;
    }
}