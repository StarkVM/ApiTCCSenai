using System.Security.Claims;
using UserAccess.Application.IdentityVerification.CreateIdentityVerificationSession;
using UserAccess.Domain.Helpers;
using Api.Common.Errors;
using UserAccess.Application.IdentityVerification.CreateIdentityVerificationSession.Records;


namespace Api.Routes.UserAccess;

public static class IdentityVerificationRoutes
{
    public static RouteGroupBuilder MapIdentityVerificationRoutes(this RouteGroupBuilder group)
    {
        var authGroup = group.MapGroup("/identity-verification");

        authGroup.MapPost("/session",IdentityVerificationRequest)
            .RequireAuthorization()
            .RequireRateLimiting("public")
            .WithName("CreateIdentityVerificationSession")
            .WithTags("Identity Verification");

        return group;
    }

    private static async Task<IResult> IdentityVerificationRequest(
        CreateIdentityVerificationSessionHandler handler,
        HttpContext httpContext,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken
    )
    {
        var logger = loggerFactory.CreateLogger(typeof(IdentityVerificationRoutes).FullName!);
        
        logger.LogInformation("Starting Request identity verification flow. RequestId: {RequestId}", httpContext.TraceIdentifier);

        var userIdString = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)?? 
                     httpContext.User.FindFirstValue("sub");
        
        Guid.TryParse(userIdString, out Guid userId);

        if (!userId.GuidIdIsValid())
        {
            return Results.Unauthorized();
        }
        
        var command = new CreateIdentityVerificationSessionCommand(userId);

        try
        {
            var result = await handler.HandleAsync(command, cancellationToken);
            
            logger.LogInformation("Request Me completed successfully. UserId: {UserId}. RequestId: {RequestId}",
                userId,
                httpContext.TraceIdentifier);
            
            return Results.Ok(new
                { 
                    verificationUrl = result.VerificationUrl
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
}