using System.Security.Claims;
using UserAccess.Application.CurrentUser.Me;
using UserAccess.Application.CurrentUser.Me.Records;
using UserAccess.Domain.Helpers;
using Api.Common.Errors;
using UserAccess.Application.CurrentUser.BecomeProvider;
using UserAccess.Application.CurrentUser.BecomeProvider.Records;
using UserAccess.Application.CurrentUser.DisableUser;
using UserAccess.Application.CurrentUser.DisableUser.Records;


namespace Api.Routes.UserAccess;

public static class UsersRoutes
{
    public static RouteGroupBuilder MapUserRoutes(this RouteGroupBuilder group)
    {
        var authGroup = group.MapGroup("/user");

        authGroup.MapGet("/me",MeRequest)
            .RequireAuthorization()
            .RequireRateLimiting("public")
            .WithName("Me")
            .WithTags("User");
        
        authGroup.MapPost("/me/provider", BecomeProviderAsync)
            .RequireAuthorization()
            .RequireRateLimiting("public")
            .WithName("BecomeProvider")
            .WithTags("User");

        authGroup.MapDelete("/me", DisableUserAsync)
            .RequireAuthorization()
            .RequireRateLimiting("public")
            .WithName("DeleteUser")
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
        var logger = loggerFactory.CreateLogger(typeof(UsersRoutes).FullName!);
        
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
                    createdAt = result.CreatedAt,
                    address = result.Address,
                });
        }
        
        catch (Exception exception)
        {
            logger.LogWarning(
                "User request me failed. Error: {Error}. RequestId: {RequestId}",
                exception.Message,
                httpContext.TraceIdentifier);

           return ApiExceptionMapper.Map(exception, httpContext);
        }
    }

    private static async Task<IResult> BecomeProviderAsync(
        BecomeProviderHandler handler,
        HttpContext httpContext,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(typeof(UsersRoutes).FullName!);

        logger.LogInformation(
            "Starting become provider flow. RequestId: {RequestId}",
            httpContext.TraceIdentifier);

        var userIdString =
            httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            httpContext.User.FindFirstValue("sub");

        if (!Guid.TryParse(userIdString, out var userId))
        {
            return Results.Unauthorized();
        }

        var command = new BecomeProviderCommand(userId);

        try
        {
            var result = await handler.HandleAsync(command, cancellationToken);

            return Results.Ok(new
            {
                success = result.Success
            });
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                "User request become provider failed. Error: {Error}. RequestId: {RequestId}",
                exception.Message,
                httpContext.TraceIdentifier);

            return ApiExceptionMapper.Map(exception, httpContext);
        }
    }
    
    private static async Task<IResult> DisableUserAsync(
        DisableUserHandler handler,
        HttpContext httpContext,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(typeof(UsersRoutes).FullName!);

        logger.LogInformation(
            "Starting disable user flow. RequestId: {RequestId}",
            httpContext.TraceIdentifier);

        var userIdString =
            httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            httpContext.User.FindFirstValue("sub");

        if (!Guid.TryParse(userIdString, out var userId))
        {
            return Results.Unauthorized();
        }

        var command = new DisableUserCommand(userId);

        try
        {
            var result = await handler.HandleAsync(command, cancellationToken);

            return Results.Ok(new
            {
                success = result.Success
            });
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                "Disable user failed. Error: {Error}. RequestId: {RequestId}",
                exception.Message,
                httpContext.TraceIdentifier);

            return ApiExceptionMapper.Map(exception, httpContext);
        }
    }
}