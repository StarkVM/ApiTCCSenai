using Microsoft.AspNetCore.Identity.Data;
using UserAccess.Application.Auth.Register;

using UserAccess.Application.Auth.Register.Records;
using Api.Routes.UserAccess.Records;
using UserAccess.Application.Auth.Login;
using UserAccess.Application.Auth.Login.Records;
using UserAccess.Application.Auth.RefreshTokens;
using UserAccess.Application.Auth.RefreshTokens.Records;
using UserAccess.Application.Auth.VerifyEmail.Records;
using UserAccess.Application.Auth.ResetPassword;
using UserAccess.Application.Auth.ResetPassword.Records;
using UserAccess.Application.Auth.VerifyEmail;
using LoginRequest = Api.Routes.UserAccess.Records.LoginRequest;

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
        
        authGroup.MapPost("/email-verification/request-new-code", RequestNewEmailVerificationCodeAsync)
            .RequireRateLimiting("public")
            .WithName("RequestNewVerificationCode")
            .WithTags("Auth");
        
        authGroup.MapPost("/email-verification/verify-email", VerifyEmailAsync)
            .RequireRateLimiting("public")
            .WithName("VerifyEmail")
            .WithTags("Auth");
        
        authGroup.MapPost("/login", LoginAsync)
            .RequireRateLimiting("public")
            .WithName("Login")
            .WithTags("Auth");
        
        authGroup.MapPost("/login/request-new-code", RequestNewLoginVerificationCodeAsync)
            .RequireRateLimiting("public")
            .WithName("RequestNewLoginVerificationCode")
            .WithTags("Auth");
        
        authGroup.MapPost("/login/verify", VerifyLoginAsync)
            .RequireRateLimiting("public")
            .WithName("VerifyLogin")
            .WithTags("Auth");
        
        authGroup.MapPost("/refresh-tokens", RefreshTokensAsync)
            .RequireRateLimiting("public")
            .WithName("RefreshTokens")
            .WithTags("Auth");
        
        return group;
    }
    
    /// <summary>
    /// REFRESH TOKENS
    /// </summary>
    
    private static async Task<IResult> RefreshTokensAsync(
        RefreshTokensRequest request,
        RefreshTokensHandler handler,
        HttpContext httpContext,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken
    )
    {
        var logger = loggerFactory.CreateLogger(typeof(AuthRoutes).FullName!);
        
        logger.LogInformation("Starting refresh tokens flow. RequestId: {RequestId}", httpContext.TraceIdentifier);
        
        var command = new RefreshTokensCommand(request.RefreshToken);

        try
        {
            var result = await handler.RefreshAsync(command, cancellationToken);

            logger.LogInformation("Refresh Token processed successfully. RequestId: {RequestId}",
                httpContext.TraceIdentifier);

            return Results.Ok(new
            {
                accessToken = result.AccessToken,
                refreshToken = result.RefreshToken,
                accessTokenExpiresAtUtc = result.AccessTokenExpiresAtUtc,
                refreshTokenExpiresAtUtc = result.RefreshTokenExpiresAtUtc,
                requestId = httpContext.TraceIdentifier
            });
        }
        catch (InvalidOperationException ex) when (ex.Message == "REFRESH_TOKEN_NOT_FOUND.")
        {
            logger.LogWarning(
                "Refresh Token failed. RequestId: {RequestId}",
                httpContext.TraceIdentifier);
            
            return Results.BadRequest(
                new
                {
                    message = "Refresh Token not found.",
                    requestId = httpContext.TraceIdentifier 
                });
        }
        catch (InvalidOperationException ex) when (ex.Message == "REFRESH_TOKEN_NOT_ACTIVE.")
        {
            logger.LogWarning(
                "Refresh Token failed. RequestId: {RequestId}",
                httpContext.TraceIdentifier);
            
            return Results.BadRequest(
                new
                {
                    message = "Refresh Token not active.",
                    requestId = httpContext.TraceIdentifier 
                });
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(
                "Refresh Token failed. Error: {Error}. RequestId: {RequestId}",
                ex.Message,
                httpContext.TraceIdentifier);

            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["request"] = new[] { ex.Message }
            });
        }
    }

    /// <summary>
    /// LOGIN
    /// </summary>

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        LoginHandler handler,
        HttpContext httpContext,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken
    )
    {
        var logger = loggerFactory.CreateLogger(typeof(AuthRoutes).FullName!);
        
        logger.LogInformation("Starting login flow. RequestId: {RequestId}", httpContext.TraceIdentifier);
        
        var command = new LoginCommand(request.Email, request.Password);

        try
        {
            var result = await handler.HandleAsync(command, cancellationToken);

            logger.LogInformation("Login verification code sent successfully. RequestId: {RequestId}",
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
                "Login request validation failed. Error: {Error}. RequestId: {RequestId}",
                ex.Message,
                httpContext.TraceIdentifier);

            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["request"] = new[] { ex.Message }
            });
        }
    }
    
    /// <summary>
    /// Login verification Code
    /// </summary>
    
    private static async Task<IResult> VerifyLoginAsync(
        VerifyLoginRequest request,
        VerifyLoginHandler handler,
        HttpContext httpContext,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken
    )
    {
        var logger = loggerFactory.CreateLogger(typeof(AuthRoutes).FullName!);
        
        logger.LogInformation("Starting login verification code flow. RequestId: {RequestId}", httpContext.TraceIdentifier);
        
        var command = new VerifyLoginCommand(request.Email, request.Code);

        try
        {
            var result = await handler.HandleAsync(command, cancellationToken);

            logger.LogInformation("Login verification code processed successfully. RequestId: {RequestId}",
                httpContext.TraceIdentifier);

            return Results.Ok(new
            {
                accessToken = result.AccessToken,
                refreshToken = result.RefreshToken,
                accessTokenExpiresAtUtc = result.AccessTokenExpiresAtUtc,
                refreshTokenExpiresAtUtc = result.RefreshTokenExpiresAtUtc,
                requestId = httpContext.TraceIdentifier
            });
        }
        catch (InvalidOperationException ex) when (ex.Message == "INVALID_CREDENTIALS")
        {
            logger.LogWarning(
                "Login verification failed. RequestId: {RequestId}",
                httpContext.TraceIdentifier);
            
            return Results.BadRequest(
                new
                {
                    message = "Unable to complete login.",
                    requestId = httpContext.TraceIdentifier 
                });
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(
                "Login verification request validation failed. Error: {Error}. RequestId: {RequestId}",
                ex.Message,
                httpContext.TraceIdentifier);

            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["request"] = new[] { ex.Message }
            });
        }
    }

    /// <summary>
    /// Request new Login verification Code
    /// </summary>

    private static async Task<IResult> RequestNewLoginVerificationCodeAsync(
        NewLoginVerificationCodeRequest request,
        RequestNewLoginVerificationCodeHandler handler,
        HttpContext httpContext,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken
    )
    {
        var logger = loggerFactory.CreateLogger(typeof(AuthRoutes).FullName!);

        logger.LogInformation("Starting request new login verification code flow. RequestId: {RequestId}",
            httpContext.TraceIdentifier);

        var command = new RequestNewLoginVerificationCodeCommand(request.Email);

        try
        {
            var result = await handler.HandleAsync(command, cancellationToken);

            logger.LogInformation("New login verification code request processed successfully. RequestId: {RequestId}",
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
                "Request new login verification code failed. Error: {Error}. RequestId: {RequestId}",
                ex.Message,
                httpContext.TraceIdentifier);

            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["request"] = new[] { ex.Message }
            });
        }
    }
    

    /// <summary>
    /// REGISTER
    /// </summary>
    /// 
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

            /*return Results.Ok(new
            {
                id = result.UserId,
                email = result.Email,
                createdAt = result.CreatedAtUtc,
                requestId = httpContext.TraceIdentifier,
            });*/

            return Results.Created($"/api/v1/user-access/users/me",new
            {
                id = result.UserId,
                email = result.Email,
                createdAt = result.CreatedAtUtc,
                requestId = httpContext.TraceIdentifier,
            }) ;
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
    }

    /// <summary>
    /// VERIFY EMAIL VERIFICATION CODE
    /// </summary>
    private static async Task<IResult> VerifyEmailAsync(
        VerifyEmailRequest request,
        VerifyEmailHandler handler,
        HttpContext httpContext,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken
    )
    {
        var logger = loggerFactory.CreateLogger(typeof(AuthRoutes).FullName!);
        
        logger.LogInformation("Starting verification code flow. RequestId: {RequestId}", httpContext.TraceIdentifier);
        
        var command = new VerifyEmailCommand(request.Email, request.Code);

        try
        {
            var result = await handler.HandleAsync(command, cancellationToken);

            logger.LogInformation("Email verification code processed successfully. RequestId: {RequestId}",
                httpContext.TraceIdentifier);

            return Results.Ok(new
            {
                accessToken = result.AccessToken,
                refreshToken = result.RefreshToken,
                accessTokenExpiresAtUtc = result.AccessTokenExpiresAtUtc,
                refreshTokenExpiresAtUtc = result.RefreshTokenExpiresAtUtc,
                requestId = httpContext.TraceIdentifier
            });
        }
        catch (InvalidOperationException ex) when (ex.Message == "INVALID_CREDENTIALS")
        {
            logger.LogWarning(
                "Email verification failed. RequestId: {RequestId}",
                httpContext.TraceIdentifier);
            
            return Results.BadRequest(
                new
                {
                    message = "Unable to verify email.",
                    requestId = httpContext.TraceIdentifier 
                });
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(
                "Email verification request validation failed. Error: {Error}. RequestId: {RequestId}",
                ex.Message,
                httpContext.TraceIdentifier);

            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["request"] = new[] { ex.Message }
            });
        }
    }
    
    
    /// <summary>
    /// REQUEST NEW EMAIL VERIFICATION CODE
    /// </summary>
   
    private static async Task<IResult> RequestNewEmailVerificationCodeAsync(
        NewRegisterEmailVerificationCodeRequest  request,
        RequestNewRegisterEmailVerificationCodeHandler handler,
        HttpContext httpContext,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken
    )
    {
        var logger = loggerFactory.CreateLogger(typeof(AuthRoutes).FullName!);
        
        logger.LogInformation("Starting request new verification code flow. RequestId: {RequestId}", httpContext.TraceIdentifier);

        var command = new RequestNewRegisterEmailVerificationCodeCommand(
            request.Email
        );

        try
        {
            var result = await  handler.HandleAsync(command, cancellationToken);
            
            logger.LogInformation("Request new email verification code processed. Success: {Success}. RequestId: {RequestId}",
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
                "User request new email verification code failed. Error: {Error}. RequestId: {RequestId}",
                ex.Message,
                httpContext.TraceIdentifier);

            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["request"] = new[] { ex.Message }
            });
        }
    }

    /// <summary>
    /// FORGOT PASSWORD
    /// </summary>
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
    
    /// <summary>
    /// RESET PASSWORD
    /// </summary>
    
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