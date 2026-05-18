using System.Security.Claims;
using System.Text;
using UserAccess.Application.IdentityVerification.CreateIdentityVerificationSession;
using UserAccess.Domain.Helpers;
using Api.Common.Errors;
using UserAccess.Application.IdentityVerification.CreateIdentityVerificationSession.Records;
using UserAccess.Application.IdentityVerification.ProcessIdentityVerificationWebhook;
using UserAccess.Application.IdentityVerification.ProcessIdentityVerificationWebhook.Records;


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
        
        authGroup.MapPost(
                "/webhook/didit",
                ProcessDiditWebhookAsync)
            .AllowAnonymous()
            .WithName("ProcessDiditIdentityVerificationWebhook")
            .WithTags("Identity Verification");

        return group;
    }

    private static async Task<IResult> ProcessDiditWebhookAsync(
        ProcessIdentityVerificationWebhookHandler handler,
        HttpContext httpContext,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(
            typeof(IdentityVerificationRoutes).FullName!);

        logger.LogInformation(
            "Starting Didit identity verification webhook flow. RequestId: {RequestId}",
            httpContext.TraceIdentifier);

        using var reader = new StreamReader(
            httpContext.Request.Body,
            Encoding.UTF8);

        var rawBody = await reader.ReadToEndAsync(cancellationToken);

        var signatureV2 = httpContext.Request.Headers["X-Signature-V2"]
            .FirstOrDefault();

        var signatureSimple = httpContext.Request.Headers["X-Signature-Simple"]
            .FirstOrDefault();

        var timestamp = httpContext.Request.Headers["X-Timestamp"]
            .FirstOrDefault();

        var command = new ProcessIdentityVerificationWebhookCommand(
            RawBody: rawBody,
            SignatureV2: signatureV2,
            SignatureSimple: signatureSimple,
            Timestamp: timestamp);

        try
        {
            var result = await handler.HandleAsync(
                command,
                cancellationToken);

            logger.LogInformation(
                "Didit identity verification webhook processed. Code: {Code}. RequestId: {RequestId}",
                result.Code,
                httpContext.TraceIdentifier);

            return Results.Ok(new
            {
                message = "Webhook event dispatched"
            });
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                "Process webhook failed. Error: {Error}. RequestId: {RequestId}",
                ex.Message,
                httpContext.TraceIdentifier);

            return ApiExceptionMapper.Map(ex, httpContext);
        }
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
                "Request identity verification url failed. Error: {Error}. RequestId: {RequestId}",
                exception.Message,
                httpContext.TraceIdentifier);

           return ApiExceptionMapper.Map(exception, httpContext);
        }
    }
}