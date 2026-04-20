using UserAccess.Infrastructure.Persistence;

namespace Api.Routes.UserAccess;

public static class UserAccessRoutes
{
    // Português
    // Registra todas as rotas relacionadas ao módulo UserAccess
    // English
    // Registers all routes related to the UserAccess module
    public static IEndpointRouteBuilder MapUserAccessRoutes(this IEndpointRouteBuilder endpoints)
    {
        // Português
        // Grupo de rotas para manter organização e facilitar versionamento
        // English
        // Route group to keep endpoints organized and allow future versioning
        var group = endpoints.MapGroup("/user-access").RequireRateLimiting("public");

        group.MapAuthRoutes();
        group.MapUserRoutes();

        group.MapGet("/health/db", async (HttpContext httpContext,UserAccessDbContext db, ILoggerFactory loggerFactory) =>
        {
            // Português
            // Cria um logger específico para este endpoint
            // English
            // Creates a logger specific to this endpoint
            var logger = loggerFactory.CreateLogger("UserAccessRoutes");
            
            // Português
            // Log indicando início da verificação do banco
            // English
            // Log indicating start of database connectivity check
            logger.LogInformation("Checking database connectivity for UserAccess module." +
                                  " RequestId: {RequestId}", httpContext.TraceIdentifier);

            try
            {
                var canConnect = await db.Database.CanConnectAsync();
            
                logger.LogInformation("Database connectivity result for UserAccess." +
                    " RequestId: {RequestId}. Connected {Connected}", httpContext.TraceIdentifier,canConnect);

                return Results.Ok( new
                {
                    database = "UserAccess",
                    connected = canConnect,
                    requestId = httpContext.TraceIdentifier
                }); 
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Error while checking UserAccess database connectivity." +
                                    " RequestId: {RequestId}", httpContext.TraceIdentifier);

                return Results.Problem(
                    title: "Error while checking UserAccess database connectivity.",
                    detail: $"Error while checking UserAccess database connectivity.",
                    statusCode: StatusCodes.Status500InternalServerError,
                    extensions: new Dictionary<string, object?>
                    {
                        ["requestId"] = httpContext.TraceIdentifier
                    });
            }
        }).RequireRateLimiting("public");
        
        return endpoints;
    }
}