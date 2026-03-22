using Microsoft.AspNetCore.Identity.Data;
using UserAccess.Application.Auth.Register;

using UserAccess.Application.Auth.Register.Records;
using Api.Routes.UserAccess.Records;
using UserAccess.Application.Auth.ResetPassword;
using UserAccess.Application.Auth.ResetPassword.Records;

namespace Api.Routes.UserAccess;

public static class AuthRoutes
{
    public static RouteGroupBuilder MapAuthRoutes(this RouteGroupBuilder group)
    {
        var authGroup = group.MapGroup("/auth");
        
        authGroup.MapPost("/register", RegisterAsync)
            .RequireRateLimiting("public")
            .WithName("RegisterUser")
            .WithTags("Auth");
        
        authGroup.MapPost("/forgot-password", ForgotPasswordAsync)
            .RequireRateLimiting("public")
            .WithName("ForgotPassword")
            .WithTags("Auth");
        
        authGroup.MapPost("/reset-password", ResetPasswordAsync)
            .RequireRateLimiting("public")
            .WithName("ResetPassword")
            .WithTags("Auth");
        
        return group;
    }

    private static async Task<IResult> RegisterAsync(
        RegisterUserRequest request,
        RegisterUserHandler handler,
        HttpContext httpContext,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken
        )
    {
        var logger = loggerFactory.CreateLogger(typeof(AuthRoutes).FullName!);
        
        logger.LogInformation("Starting user registration. RequestId: {requestId}", httpContext.TraceIdentifier);
        
        var address = new RegisterUserAddress(
            request.Address.State,
            request.Address.City,
            request.Address.District,
            request.Address.Street,
            request.Address.ZipCode
        );

        var command = new RegisterUserCommand(
            request.FirstName,
            request.LastName,
            request.BirthDate,
            request.Email,
            request.Cpf,
            request.Password,
            address
        );
        

        try
        {
            var result = await handler.HandleAsync(command, cancellationToken);

            logger.LogInformation("User registration completed successfully. UserId: {UserId}. RequestId: {RequestId}",
                result.UserId,
                httpContext.TraceIdentifier);

            return Results.Ok(new
            {
                id = result.UserId,
                email = result.Email,
                createdAt = result.CreatedAtUtc,
                requestId = httpContext.TraceIdentifier,
            });

            /*return Results.Created($"/api/v1/user-access/users/{result.UserId}",new
            {
                id = result.UserId,
                email = result.Email,
                createdAt = result.CreatedAtUtc,
                requestId = httpContext.TraceIdentifier,
            }) ;*/
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(
                "User registration validation failed. Error: {Error}. RequestId: {RequestId}",
                ex.Message,
                httpContext.TraceIdentifier);

            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["register"] = new[] { ex.Message }
            });
        }
        catch (InvalidOperationException ex) when (ex.Message == "EMAIL_ALREADY_REGISTERED")
        {
            logger.LogWarning(
                "User registration failed because email already exists. RequestId: {RequestId}",
                httpContext.TraceIdentifier);
            
            return Results.Conflict(
                new
                {
                    message = "Email already exists.",
                    requestId = httpContext.TraceIdentifier
                }
                );
        }
        catch (InvalidOperationException ex) when (ex.Message == "CPF_ALREADY_REGISTERED")
        {
            logger.LogWarning(
                "User registration failed because CPF already exists. RequestId: {RequestId}",
                httpContext.TraceIdentifier);
            
            return Results.Conflict(
                new
                {
                    message = "CPF already exists.",
                    requestId = httpContext.TraceIdentifier 
                });
        }
    }

    private static async Task<IResult> ForgotPasswordAsync(
        ForgotPasswordRequest  request,
        RequestPasswordResetHandler handler,
        HttpContext httpContext,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken
        )
    {
        var logger = loggerFactory.CreateLogger(typeof(AuthRoutes).FullName!);
        
        logger.LogInformation("Starting forgot password request flow. RequestId: {RequestId}", httpContext.TraceIdentifier);

        var command = new RequestPasswordResetCommand(
            request.Email
        );

        try
        {
            var result = await  handler.HandleAsync(command, cancellationToken);
            
            logger.LogInformation("Forgot password request processed. Success: {Success}. RequestId: {RequestId}",
                result.Success,
                httpContext.TraceIdentifier);
            
            return Results.Ok(new
            {
                success = result.Success,
                requestId = httpContext.TraceIdentifier
            });
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(
                "User forgot-password request failed. Error: {Error}. RequestId: {RequestId}",
                ex.Message,
                httpContext.TraceIdentifier);

            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["request"] = new[] { ex.Message }
            });
        }
    }
    
    private static async Task<IResult> ResetPasswordAsync(
        ResetUserPasswordRequest  request,
        ResetPasswordHandler handler,
        HttpContext httpContext,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken
    )
    {
        var logger = loggerFactory.CreateLogger(typeof(AuthRoutes).FullName!);
        
        logger.LogInformation("Starting reset password request flow. RequestId: {RequestId}", httpContext.TraceIdentifier);

        var command = new ResetPasswordCommand(
            request.Email,
            request.NewPassword,
            request.Code
        );

        try
        {
            var result = await  handler.HandleAsync(command, cancellationToken);
            
            logger.LogInformation("Reset password request processed. Success: {Success}. RequestId: {RequestId}",
                result.Success,
                httpContext.TraceIdentifier);
            
            return Results.Ok(new
            {
                success = result.Success,
                requestId = httpContext.TraceIdentifier
            });
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(
                "User reset password request failed. Error: {Error}. RequestId: {RequestId}",
                ex.Message,
                httpContext.TraceIdentifier);

            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["request"] = new[] { ex.Message }
            });
        }
    }
}