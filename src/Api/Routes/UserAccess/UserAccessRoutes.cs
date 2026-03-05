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
        var group = endpoints.MapGroup("/user-access");

        group.MapGet("/health/db", async (UserAccessDbContext db) =>
        {
            var canConnect = await db.Database.CanConnectAsync();

            return Results.Ok( new
            {
                    database = "UserAccess",
                    connected = canConnect
            });
        });
        return endpoints;
    }
}