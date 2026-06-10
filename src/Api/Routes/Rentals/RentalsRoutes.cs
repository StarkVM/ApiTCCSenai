using System.Security.Claims;
using Api.Common.Errors;
using Api.Routes.Rentals.Records;
using Rentals.Application.CompleteRental;
using Rentals.Application.CompleteRental.Records;
using Rentals.Application.CreateRental;
using Rentals.Application.CreateRental.Records;

namespace Api.Routes.Rentals;

/// <summary>
/// Defines the HTTP routes of the Rentals module.
/// / Define as rotas HTTP do módulo de aluguéis.
/// </summary>
public static class RentalsRoutes
{
    /// <summary>
    /// Maps all Rentals module routes.
    /// / Mapeia todas as rotas do módulo de aluguéis.
    /// </summary>
    public static IEndpointRouteBuilder MapRentalsRoutes(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/rentals")
            .RequireAuthorization()
            .RequireRateLimiting("public")
            .WithTags("Rentals");

        group.MapPost("/", CreateRentalAsync)
            .WithName("CreateRental")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
        
        group.MapPost("/{rentalId:guid}/complete", CompleteRentalAsync)
            .WithName("CompleteRental")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

        return endpoints;
    }
    
        /// <summary>
    /// Completes a rental when requested by its provider or renter.
    /// / Finaliza um aluguel quando solicitado pelo fornecedor ou locatário.
    /// </summary>
    private static async Task<IResult> CompleteRentalAsync(
        Guid rentalId,
        HttpContext httpContext,
        CompleteRentalHandler handler,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(
            typeof(RentalsRoutes).FullName!);

        logger.LogInformation(
            "Starting complete rental endpoint flow. RentalId: {RentalId}, RequestId: {RequestId}",
            rentalId,
            httpContext.TraceIdentifier);

        var requesterId = GetAuthenticatedUserId(httpContext);

        if (requesterId is null)
        {
            logger.LogWarning(
                "Complete rental request rejected because authenticated user id is invalid. RentalId: {RentalId}, RequestId: {RequestId}",
                rentalId,
                httpContext.TraceIdentifier);

            return Results.Unauthorized();
        }

        try
        {
            var command = new CompleteRentalCommand(
                rentalId,
                requesterId.Value);

            var result = await handler.HandleAsync(
                command,
                cancellationToken);

            logger.LogInformation(
                "Complete rental endpoint flow completed successfully. RentalId: {RentalId}, RequesterId: {RequesterId}, RequestId: {RequestId}",
                result.RentalId,
                requesterId.Value,
                httpContext.TraceIdentifier);

            return Results.Ok(new
            {
                id = result.RentalId,
                listingId = result.ListingId,
                providerId = result.ProviderId,
                renterId = result.RenterId,
                status = result.Status.ToString(),
                completedByUserId = result.CompletedByUserId,
                completedAtUtc = result.CompletedAtUtc,
                requestId = httpContext.TraceIdentifier
            });
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Complete rental endpoint flow failed. RentalId: {RentalId}, RequesterId: {RequesterId}, RequestId: {RequestId}",
                rentalId,
                requesterId.Value,
                httpContext.TraceIdentifier);

            return ApiExceptionMapper.Map(
                exception,
                httpContext);
        }
    }

    /// <summary>
    /// Creates an immediately approved rental based on a listing.
    /// / Cria um aluguel aprovado imediatamente com base em um anúncio.
    /// </summary>
    private static async Task<IResult> CreateRentalAsync(
        CreateRentalRequest request,
        HttpContext httpContext,
        CreateRentalHandler handler,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(
            typeof(RentalsRoutes).FullName!);

        logger.LogInformation(
            "Starting create rental endpoint flow. ListingId: {ListingId}, RequestId: {RequestId}",
            request.ListingId,
            httpContext.TraceIdentifier);

        var renterId = GetAuthenticatedUserId(httpContext);

        if (renterId is null)
        {
            logger.LogWarning(
                "Create rental request rejected because authenticated user id is invalid. ListingId: {ListingId}, RequestId: {RequestId}",
                request.ListingId,
                httpContext.TraceIdentifier);

            return Results.Unauthorized();
        }

        try
        {
            var command = new CreateRentalCommand(
                request.ListingId,
                renterId.Value,
                request.StartDate,
                request.EndDate,
                request.IncludeOperator,
                request.IncludeFreight);

            var result = await handler.HandleAsync(
                command,
                cancellationToken);

            logger.LogInformation(
                "Create rental endpoint flow completed successfully. RentalId: {RentalId}, ListingId: {ListingId}, RenterId: {RenterId}, RequestId: {RequestId}",
                result.RentalId,
                result.ListingId,
                result.RenterId,
                httpContext.TraceIdentifier);

            return Results.Created(
                $"/api/v1/rentals/{result.RentalId}",
                new
                {
                    id = result.RentalId,
                    listingId = result.ListingId,
                    ownerId = result.OwnerId,
                    renterId = result.RenterId,
                    status = result.Status.ToString(),

                    startDate = result.StartDate,
                    endDate = result.EndDate,
                    totalDays = result.TotalDays,

                    includeOperator = result.IncludeOperator,
                    includeFreight = result.IncludeFreight,

                    machineSubtotal = result.MachineSubtotal,
                    operatorSubtotal = result.OperatorSubtotal,
                    freightSubtotal = result.FreightSubtotal,
                    totalAmount = result.TotalAmount,

                    createdAtUtc = result.CreatedAtUtc,
                    requestId = httpContext.TraceIdentifier
                });
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Create rental endpoint flow failed. ListingId: {ListingId}, RenterId: {RenterId}, RequestId: {RequestId}",
                request.ListingId,
                renterId.Value,
                httpContext.TraceIdentifier);

            return ApiExceptionMapper.Map(
                exception,
                httpContext);
        }
    }

    /// <summary>
    /// Gets the authenticated user identifier from the access token.
    /// / Obtém o identificador do usuário autenticado pelo access token.
    /// </summary>
    private static Guid? GetAuthenticatedUserId(
        HttpContext httpContext)
    {
        var userIdValue =
            httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            httpContext.User.FindFirstValue("sub");

        if (Guid.TryParse(userIdValue, out var userId))
        {
            return userId;
        }
        
        return null;
    }
}