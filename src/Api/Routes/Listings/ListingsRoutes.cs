using System.Globalization;
using System.Security.Claims;
using Api.Common.Errors;
using Api.Routes.Listings.Requests;
using Listings.Application.CreateListing;
using Listings.Application.CreateListings.Records;
using Listings.Application.DeleteListing;
using Listings.Application.DeleteListing.Records;
using Listings.Application.GetListings;
using Listings.Application.GetListings.Records;
using Listings.Domain.Enums;

namespace Api.Routes.Listings;

public static class ListingsRoutes
{
    public static IEndpointRouteBuilder MapListingsRoutes(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/listings")
            .RequireRateLimiting("public");

        group.MapPost("/", CreateListingAsync)
            .RequireAuthorization()
            .RequireRateLimiting("public")
            .WithName("CreateListing")
            .WithTags("Listings");
        
        group.MapDelete("/{listingId:guid}", DeleteListingAsync)
            .RequireAuthorization()
            .RequireRateLimiting("public")
            .WithName("DeleteListing")
            .WithTags("Listings");
        
        group.MapGet("/", GetListingsAsync)
            .AllowAnonymous()
            .WithName("GetListings")
            .WithTags("Listings")
            .Produces<GetListingsResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        return endpoints;
    }
    
    /// <summary>
    /// Searches public listings or the authenticated user's listings.
    /// / Pesquisa anúncios públicos ou os anúncios do usuário autenticado.
    /// </summary>
    private static async Task<IResult> GetListingsAsync(
        [AsParameters] GetListingsRequest request,
        HttpContext httpContext,
        GetListingsHandler handler,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(
            typeof(ListingsRoutes).FullName!);

        var requesterId = GetAuthenticatedUserId(httpContext);

        var mine = request.Mine ?? false;
        var page = request.Page ?? 1;
        var pageSize = request.PageSize ?? 20;

        if (mine && requesterId is null)
        {
            logger.LogWarning(
                "Own listings search rejected because the user is not authenticated. RequestId: {RequestId}",
                httpContext.TraceIdentifier);

            return Results.Unauthorized();
        }

        try
        {
            var query = new GetListingsQuery(
                requesterId,
                mine,
                request.Name,
                request.Category,
                request.Status,
                page,
                pageSize);

            var result = await handler.HandleAsync(
                query,
                cancellationToken);

            return Results.Ok(result);
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Get listings flow failed. Mine: {Mine}, RequestId: {RequestId}",
                mine,
                httpContext.TraceIdentifier);

            return ApiExceptionMapper.Map(
                exception,
                httpContext);
        }
    }
    
    /// <summary>
/// Disables a listing using soft delete.
/// / Desativa um anúncio usando exclusão lógica.
/// </summary>
    private static async Task<IResult> DeleteListingAsync(
        Guid listingId,
        HttpContext httpContext,
        DeleteListingHandler handler,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(typeof(ListingsRoutes).FullName!);

        logger.LogInformation(
            "Starting delete listing endpoint flow. ListingId: {ListingId}, RequestId: {RequestId}",
            listingId,
            httpContext.TraceIdentifier);

        var requesterId = GetAuthenticatedUserId(httpContext);

        if (requesterId is null)
        {
            logger.LogWarning(
                "Delete listing failed because authenticated user id was not found. ListingId: {ListingId}, RequestId: {RequestId}",
                listingId,
                httpContext.TraceIdentifier);

            return Results.Unauthorized();
        }

        try
        {
            var command = new DeleteListingCommand(
                listingId,
                requesterId.Value);

            var result = await handler.HandleAsync(
                command,
                cancellationToken);

            logger.LogInformation(
                "Delete listing endpoint flow completed successfully. ListingId: {ListingId}, RequesterId: {RequesterId}, RequestId: {RequestId}",
                result.ListingId,
                requesterId.Value,
                httpContext.TraceIdentifier);

            return Results.Ok(new
            {
                id = result.ListingId,
                status = result.Status.ToString(),
                updatedAtUtc = result.UpdatedAtUtc
            });
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Delete listing failed. ListingId: {ListingId}, RequesterId: {RequesterId}, RequestId: {RequestId}",
                listingId,
                requesterId.Value,
                httpContext.TraceIdentifier);

            return ApiExceptionMapper.Map(
                exception,
                httpContext);
        }
    }

    private static async Task<IResult> CreateListingAsync(
        HttpContext httpContext,
        CreateListingHandler handler,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(typeof(ListingsRoutes).FullName!);

        logger.LogInformation(
            "Starting create listing flow. RequestId: {RequestId}",
            httpContext.TraceIdentifier);

        var ownerId = GetAuthenticatedUserId(httpContext);

        if (ownerId is null)
        {
            logger.LogWarning(
                "Create listing request rejected because user id claim is invalid. RequestId: {RequestId}",
                httpContext.TraceIdentifier);

            return Results.Unauthorized();
        }

        if (!httpContext.Request.HasFormContentType)
        {
            logger.LogWarning(
                "Create listing request rejected because content type is not multipart/form-data. RequestId: {RequestId}",
                httpContext.TraceIdentifier);

            return Results.BadRequest(new
            {
                code = "INVALID_CONTENT_TYPE",
                message = "Request must be multipart/form-data.",
                requestId = httpContext.TraceIdentifier
            });
        }

        var openedStreams = new List<Stream>();

        try
        {
            logger.LogInformation(
                "Create listing request content info. ContentType: {ContentType}, ContentLength: {ContentLength}",
                httpContext.Request.ContentType,
                httpContext.Request.ContentLength);
            
            var form = await httpContext.Request.ReadFormAsync(cancellationToken);

            var command = CreateCommandFromForm(
                ownerId.Value,
                form,
                openedStreams,
                logger);

            var result = await handler.HandleAsync(
                command,
                cancellationToken);

            logger.LogInformation(
                "Listing created successfully. ListingId: {ListingId}. RequestId: {RequestId}",
                result.ListingId,
                httpContext.TraceIdentifier);

            return Results.Created(
                $"/api/v1/listings/{result.ListingId}",
                new
                {
                    id = result.ListingId,
                    status = result.Status.ToString(),
                    createdAtUtc = result.CreatedAtUtc,
                    requestId = httpContext.TraceIdentifier
                });
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Create listing failed. RequestId: {RequestId}",
                httpContext.TraceIdentifier);

            return ApiExceptionMapper.Map(exception, httpContext);
        }
        finally
        {
            foreach (var stream in openedStreams)
            {
                await stream.DisposeAsync();
            }
        }
    }

    private static CreateListingCommand CreateCommandFromForm(
        Guid ownerId,
        IFormCollection form,
        List<Stream> openedStreams,
        ILogger logger)
    {
        logger.LogInformation(
            "Create listing form files received. TotalFilesCount: {TotalFilesCount}. FileFields: {FileFields}",
            form.Files.Count,
            string.Join(", ", form.Files.Select(file =>
                $"Name={file.Name}; FileName={file.FileName}; Length={file.Length}; ContentType={file.ContentType}")));

        var images = form.Files
            .Where(file =>
                string.Equals(file.Name, "images", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(file.Name, "images[]", StringComparison.OrdinalIgnoreCase))
            .ToList();

        logger.LogInformation(
            "Create listing images extracted. ImagesCount: {ImagesCount}",
            images.Count);

        var imageCommands = new List<CreateListingImageCommand>();

        foreach (var image in images)
        {
            var stream = image.OpenReadStream();
            openedStreams.Add(stream);

            imageCommands.Add(new CreateListingImageCommand(
                image.FileName,
                image.ContentType,
                image.Length,
                stream));
        }

        return new CreateListingCommand(
            ownerId,
            GetRequiredFormValue(form, "title"),
            GetRequiredFormValue(form, "description"),
            GetRequiredCategory(form, "category"),
            GetRequiredDecimal(form, "dailyPrice"),
            new CreateListingPickupAddressCommand(
                GetRequiredFormValue(form, "pickupState"),
                GetRequiredFormValue(form, "pickupCity"),
                GetRequiredFormValue(form, "pickupDistrict"),
                GetRequiredFormValue(form, "pickupStreet"),
                GetRequiredFormValue(form, "pickupNumber"),
                GetRequiredFormValue(form, "pickupZipCode"),
                GetOptionalFormValue(form, "pickupComplement")),
            new CreateListingOperatorOptionCommand(
                GetRequiredBoolean(form, "operatorAvailable"),
                GetDecimalOrZero(form, "operatorDailyPrice")),
            new CreateListingFreightOptionCommand(
                GetRequiredBoolean(form, "freightAvailable"),
                GetDecimalOrZero(form, "freightFixedPrice")),
            imageCommands,
            GetRequiredBoolean(form, "isFleet"));
    }

    private static Guid? GetAuthenticatedUserId(HttpContext httpContext)
    {
        var userIdString =
            httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            httpContext.User.FindFirstValue("sub");

        if (!Guid.TryParse(userIdString, out var userId))
        {
            return null;
        }

        return userId;
    }

    private static string GetRequiredFormValue(
        IFormCollection form,
        string key)
    {
        var value = form[key].ToString();

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{key.ToUpperInvariant()}_REQUIRED");
        }

        return value.Trim();
    }

    private static string? GetOptionalFormValue(
        IFormCollection form,
        string key)
    {
        var value = form[key].ToString();

        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }
        else
        {
            return value.Trim();
        }
    }

    private static ListingCategory GetRequiredCategory(
        IFormCollection form,
        string key)
    {
        var value = GetRequiredFormValue(form, key);

        if (int.TryParse(value, out var categoryNumber) &&
            Enum.IsDefined(typeof(ListingCategory), categoryNumber))
        {
            return (ListingCategory)categoryNumber;
        }

        if (Enum.TryParse<ListingCategory>(
                value,
                ignoreCase: true,
                out var category) &&
            Enum.IsDefined(category))
        {
            return category;
        }

        throw new ArgumentException("CATEGORY_INVALID");
    }

    private static decimal GetRequiredDecimal(
        IFormCollection form,
        string key)
    {
        var value = GetRequiredFormValue(form, key);

        if (TryParseDecimal(value, out var decimalValue))
        {
            return decimalValue;
        }

        throw new ArgumentException($"{key.ToUpperInvariant()}_INVALID");
    }

    private static decimal GetDecimalOrZero(
        IFormCollection form,
        string key)
    {
        var value = form[key].ToString();

        if (string.IsNullOrWhiteSpace(value))
        {
            return 0;
        }

        if (TryParseDecimal(value, out var decimalValue))
        {
            return decimalValue;
        }

        throw new ArgumentException($"{key.ToUpperInvariant()}_INVALID");
    }

    private static bool TryParseDecimal(
        string value,
        out decimal result)
    {
        if (decimal.TryParse(
                value,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out result))
        {
            return true;
        }

        return decimal.TryParse(
            value,
            NumberStyles.Number,
            new CultureInfo("pt-BR"),
            out result);
    }

    private static bool GetRequiredBoolean(
        IFormCollection form,
        string key)
    {
        var value = GetRequiredFormValue(form, key);

        if (bool.TryParse(value, out var boolValue))
        {
            return boolValue;
        }

        if (value == "1")
        {
            return true;
        }

        if (value == "0")
        {
            return false;
        }

        throw new ArgumentException($"{key.ToUpperInvariant()}_INVALID");
    }
}