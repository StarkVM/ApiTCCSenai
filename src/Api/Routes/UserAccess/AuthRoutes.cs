using Microsoft.AspNetCore.Identity.Data;
using UserAccess.Application.Auth.Register;
using UserAccess.Application.Auth.Register.Records;
using Api.Routes.UserAccess.AuthRecords;
using UserAccess.Application.Auth.Login;
using UserAccess.Application.Auth.Login.Records;
using UserAccess.Application.Auth.Logout;
using UserAccess.Application.Auth.Logout.Records;
using UserAccess.Application.Auth.RefreshTokens;
using UserAccess.Application.Auth.RefreshTokens.Records;
using UserAccess.Application.Auth.VerifyEmail.Records;
using UserAccess.Application.Auth.ResetPassword;
using UserAccess.Application.Auth.ResetPassword.Records;
using UserAccess.Application.Auth.VerifyEmail;
using LoginRequest = Api.Routes.UserAccess.AuthRecords.LoginRequest;
using System.Security.Claims;
using Api.Common.Errors;
using UserAccess.Domain.Helpers;

namespace Api.Routes.UserAccess;

public static class AuthRoutes
{
    public static RouteGroupBuilder MapAuthRoutes(this RouteGroupBuilder group)
    {
        var authGroup = group.MapGroup("/auth");
        
        authGroup.MapPost("/refresh-tokens", RefreshTokensAsync)
            .RequireRateLimiting("public")
            .WithName("RefreshTokens")
            .WithTags("Auth");
        
        authGroup.MapPost("/register", RegisterAsync)
            .RequireRateLimiting("public")
            .WithName("RegisterUser")
            .WithTags("Auth");
        
        authGroup.MapPost("/email-verification/verify-email", VerifyEmailAsync)
            .RequireRateLimiting("public")
            .WithName("VerifyEmail")
            .WithTags("Auth");
        
        authGroup.MapPost("/email-verification/request-new-code", RequestNewEmailVerificationCodeAsync)
            .RequireRateLimiting("public")
            .WithName("RequestNewVerificationCode")
            .WithTags("Auth");
        
        authGroup.MapPost("/login", LoginAsync)
            .RequireRateLimiting("public")
            .WithName("Login")
            .WithTags("Auth");
        
        authGroup.MapPost("/login/verify", VerifyLoginAsync)
            .RequireRateLimiting("public")
            .WithName("VerifyLogin")
            .WithTags("Auth");
        
        authGroup.MapPost("/login/request-new-code", RequestNewLoginVerificationCodeAsync)
            .RequireRateLimiting("public")
            .WithName("RequestNewLoginVerificationCode")
            .WithTags("Auth");
        
        authGroup.MapPost("/forgot-password", ForgotPasswordAsync)
            .RequireRateLimiting("public")
            .WithName("ForgotPassword")
            .WithTags("Auth");
        
        authGroup.MapPost("/reset-password", ResetPasswordAsync)
            .RequireRateLimiting("public")
            .WithName("ResetPassword")
            .WithTags("Auth");
        
        authGroup.MapPost("/logout-current-session", LogoutCurrentSessionAsync)
            .RequireRateLimiting("public")
            .WithName("LogoutCurrentSession")
            .WithTags("Auth");
        
        authGroup.MapPost("/logout-all-sessions", LogoutAllSessionsAsync)
            .RequireAuthorization()
            .RequireRateLimiting("public")
            .WithName("LogoutAllSessions")
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
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Refresh tokens failed. RequestId: {RequestId}", httpContext.TraceIdentifier);

            return ApiExceptionMapper.Map(exception, httpContext);
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
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Login verification failed. RequestId: {RequestId}", httpContext.TraceIdentifier);

            return ApiExceptionMapper.Map(exception, httpContext);
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
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Login failed. RequestId: {RequestId}", httpContext.TraceIdentifier);

            return ApiExceptionMapper.Map(exception, httpContext);
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
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Request new login verification code failed. RequestId: {RequestId}",
                httpContext.TraceIdentifier);

            return ApiExceptionMapper.Map(exception, httpContext);
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
        catch (Exception exception)
        {
            logger.LogWarning(exception, "User registration failed. RequestId: {RequestId}", httpContext.TraceIdentifier);

            return ApiExceptionMapper.Map(exception, httpContext);
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
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Email verification failed. RequestId: {RequestId}", httpContext.TraceIdentifier);

            return ApiExceptionMapper.Map(exception, httpContext);
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
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Request new email verification code failed. RequestId: {RequestId}",
                httpContext.TraceIdentifier);

            return ApiExceptionMapper.Map(exception, httpContext);
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
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Forgot password request failed. RequestId: {RequestId}",
                httpContext.TraceIdentifier);

            return ApiExceptionMapper.Map(exception, httpContext);
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
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Reset password request failed. RequestId: {RequestId}",
                httpContext.TraceIdentifier);

            return ApiExceptionMapper.Map(exception, httpContext);
        }
    }
    
    /// <summary>
    /// Logout current session
    /// </summary>
   
    
    private static async Task<IResult> LogoutCurrentSessionAsync(
        LogoutCurrentSessionRequest  request,
        LogoutCurrentSessionHandler handler,
        HttpContext httpContext,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken
    )
    {
        var logger = loggerFactory.CreateLogger(typeof(AuthRoutes).FullName!);
        
        logger.LogInformation("Starting logout current session flow. RequestId: {RequestId}", httpContext.TraceIdentifier);

        var command = new LogoutCurrentSessionCommand(
           request.RefreshToken
        );

        try
        {
            var result = await  handler.HandleAsync(command, cancellationToken);
            
            logger.LogInformation("Logout processed. Success: {Success}. RequestId: {RequestId}",
                result.Success,
                httpContext.TraceIdentifier);
            
            return Results.Ok(new
            {
                success = result.Success,
                requestId = httpContext.TraceIdentifier
            });
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Logout current session failed. RequestId: {RequestId}",
                httpContext.TraceIdentifier);

            return ApiExceptionMapper.Map(exception, httpContext);
        }
    }
    
    /// <summary>
    /// Logout all sessions
    /// </summary>
    
    
    private static async Task<IResult> LogoutAllSessionsAsync(
        LogoutAllSessionsHandler handler,
        HttpContext httpContext,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken
    )
    {
        var logger = loggerFactory.CreateLogger(typeof(AuthRoutes).FullName!);
        
        logger.LogInformation("Starting logout all sessions flow. RequestId: {RequestId}", httpContext.TraceIdentifier);
        
        var userIdString = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)?? 
                           httpContext.User.FindFirstValue("sub");
        
        Guid.TryParse(userIdString, out Guid userId);

        if (!userId.GuidIdIsValid())
        {
            logger.LogWarning(
                "Unauthorized logout all sessions request due to invalid user id. RequestId: {RequestId}",
                httpContext.TraceIdentifier);
            return Results.Unauthorized();
        }

        var command = new LogoutAllSessionsCommand(
           userId
        );

        try
        {
            var result = await  handler.HandleAsync(command, cancellationToken);
            
            logger.LogInformation("Logout processed. Success: {Success}. RequestId: {RequestId}",
                result.Success,
                httpContext.TraceIdentifier);
            
            return Results.Ok(new
            {
                success = result.Success,
                requestId = httpContext.TraceIdentifier
            });
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Logout all sessions failed. RequestId: {RequestId}",
                httpContext.TraceIdentifier);

            return ApiExceptionMapper.Map(exception, httpContext);
        }
    }
}