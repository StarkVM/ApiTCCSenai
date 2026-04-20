using System.Security.Claims;
using UserAccess.Application.CurrentUser.Me;
using UserAccess.Application.CurrentUser.Me.Records;
using UserAccess.Domain.Helpers;


namespace Api.Routes.UserAccess;

public static class UserRoutes
{
    public static RouteGroupBuilder MapUserRoutes(this RouteGroupBuilder group)
    {
        var authGroup = group.MapGroup("/user");

        authGroup.MapGet("/me",MeRequest)
            .RequireAuthorization()
            .RequireRateLimiting("public")
            .WithName("Me")
            .WithTags("User");

        return group;
    }

    private static async Task<IResult> MeRequest(
        MeHandler handler,
        HttpContext httpContext,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken
    )
    {
        var logger = loggerFactory.CreateLogger(typeof(UserRoutes).FullName!);
        
        logger.LogInformation("Starting Request Me flow. RequestId: {RequestId}", httpContext.TraceIdentifier);

        var userIdString = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)?? 
                     httpContext.User.FindFirstValue("sub");
        
        Guid.TryParse(userIdString, out Guid userId);

        if (!userId.GuidIdIsValid())
        {
            return Results.Unauthorized();
        }
        
        var command = new MeCommand(userId);

        try
        {
            var result = await handler.HandleAsync(command, cancellationToken);
            
            logger.LogInformation("Request Me completed successfully. UserId: {UserId}. RequestId: {RequestId}",
                result.Id,
                httpContext.TraceIdentifier);
            
            return Results.Ok(new
                { 
                    id = result.Id,
                    firstName = result.FirstName,
                    lastName = result.LastName,
                    birthDate = result.BirthDate,
                    email = result.Email,
                    status = result.Status,
                    type = result.Type,
                    address = result.Address,
                });
        }
        catch (InvalidOperationException ex) when (ex.Message == "ADDRESS_NOT_FOUND")
        {
            logger.LogWarning(
                "Request me failed because address not found failed RequestId: {RequestId}",
                httpContext.TraceIdentifier);
            
            return Results.NotFound(
                new
                {
                    message = "Address not found.",
                    requestId = httpContext.TraceIdentifier 
                });
        }
        catch (InvalidOperationException ex) when (ex.Message == "USER_NOT_FOUND")
        {
            logger.LogWarning(
                "Request me failed because user not found failed RequestId: {RequestId}",
                httpContext.TraceIdentifier);
            
            return Results.NotFound(
            new
                {
                    message = "User not found.",
                    requestId = httpContext.TraceIdentifier
                }
            );
        }
        catch (ArgumentException ex) when (ex.Message == "ID_IS_REQUIRED")
        {
            logger.LogWarning(
                "Id is required to request me RequestId: {RequestId}",
                httpContext.TraceIdentifier);
            
            return Results.BadRequest(
                    new
                    {
                        message = "Id is required failed.",
                        requestId = httpContext.TraceIdentifier
                    });
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                "User registration validation failed. Error: {Error}. RequestId: {RequestId}",
                ex.Message,
                httpContext.TraceIdentifier);

            return Results.Json(new Dictionary<string, string[]>
            {
                ["register"] = new[] { ex.Message }
            },
            statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}