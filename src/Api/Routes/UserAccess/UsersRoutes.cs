using System.Security.Claims;
using UserAccess.Application.CurrentUser.Me;
using UserAccess.Application.CurrentUser.Me.Records;
using UserAccess.Domain.Helpers;
using Api.Common.Errors;
using UserAccess.Application.CurrentUser.BecomeProvider;
using UserAccess.Application.CurrentUser.BecomeProvider.Records;
using UserAccess.Application.CurrentUser.DisableUser;
using UserAccess.Application.CurrentUser.DisableUser.Records;
using UserAccess.Application.ProfilePhotos.GetProfilePhoto;
using UserAccess.Application.ProfilePhotos.GetProfilePhoto.Records;
using UserAccess.Application.ProfilePhotos.UpdateProfilePhoto;
using UserAccess.Application.ProfilePhotos.UpdateProfilePhoto.Records;
using UserAccess.Domain.Exceptions.UserAccessExceptions;
using UserAccess.Domain.Files;


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
        
        group.MapPost("/profile/photo", UpdateProfilePhotoAsync)
            .RequireAuthorization()
            .RequireRateLimiting("public")
            .DisableAntiforgery()
            .WithName("UpdateProfilePhoto")
            .WithTags("User")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/profile/photo", GetProfilePhotoAsync)
            .RequireAuthorization()
            .RequireRateLimiting("public")
            .WithName("GetProfilePhoto")
            .WithTags("User")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        return group;
    }
    
    /// <summary>
    /// Gets the authenticated user's profile photo.
    /// / Obtém a foto de perfil do usuário autenticado.
    /// </summary>
    private static async Task<IResult> GetProfilePhotoAsync(
        HttpContext httpContext,
        GetProfilePhotoHandler handler,
        CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId(
            httpContext);

        if (userId is null)
        {
            return Results.Unauthorized();
        }

        try
        {
            var query = new GetProfilePhotoQuery(
                userId.Value);

            var result = await handler.HandleAsync(
                query,
                cancellationToken);

            return Results.Ok(new
            {
                userId = result.UserId,
                hasPhoto = result.HasPhoto,
                url = result.Url,
                urlExpiresAtUtc = result.UrlExpiresAtUtc,
                updatedAtUtc = result.UpdatedAtUtc,
                requestId = httpContext.TraceIdentifier
            });
        }
        catch (Exception exception)
        {
            return ApiExceptionMapper.Map(
                exception,
                httpContext);
        }
    }
    
    /// <summary>
    /// Updates the authenticated user's profile photo.
    /// / Atualiza a foto de perfil do usuário autenticado.
    /// </summary>
    private static async Task<IResult> UpdateProfilePhotoAsync(
        HttpContext httpContext,
        UpdateProfilePhotoHandler handler,
        CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId(
            httpContext);

        if (userId is null)
        {
            return Results.Unauthorized();
        }

        if (!httpContext.Request.HasFormContentType)
        {
            return Results.BadRequest(new
            {
                code = "MULTIPART_FORM_DATA_REQUIRED",
                message = "The request must use multipart/form-data.",
                requestId = httpContext.TraceIdentifier
            });
        }

        var form = await httpContext.Request.ReadFormAsync(
            cancellationToken);

        var file = form.Files.GetFile("photo");

        if (file is null)
        {
            return Results.BadRequest(new
            {
                code = "PROFILE_PHOTO_REQUIRED",
                message = "Profile photo is required.",
                requestId = httpContext.TraceIdentifier
            });
        }

        try
        {
            var photo = new UserProfilePhotoUpload(
                file.FileName,
                file.ContentType,
                file.Length,
                file.OpenReadStream);

            var command = new UpdateProfilePhotoCommand(
                userId.Value,
                photo);

            var result = await handler.HandleAsync(
                command,
                cancellationToken);

            return Results.Ok(new
            {
                userId = result.UserId,
                hasPhoto = result.HasPhoto,
                url = result.Url,
                urlExpiresAtUtc = result.UrlExpiresAtUtc,
                updatedAtUtc = result.UpdatedAtUtc,
                requestId = httpContext.TraceIdentifier
            });
        }
        catch (Exception exception)
        {
            return ApiExceptionMapper.Map(
                exception,
                httpContext);
        }
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
    
    private static Guid? GetAuthenticatedUserId(HttpContext httpContext)
    {
        var userIdClaim =
            httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            httpContext.User.FindFirstValue("sub");

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            throw new InvalidUserIdException();
        }

        return userId;
    }
}