using Listings.Application.Common.Exceptions;
using Listings.Application.DeleteListing.Records;
using Listings.Domain.Exceptions.ListingsExceptions;
using Listings.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Listings.Application.DeleteListing;

/// <summary>
/// Handles the delete listing use case using soft delete.
/// / Manipula o caso de uso de exclusão de anúncio usando exclusão lógica.
/// </summary>
public sealed class DeleteListingHandler
{
    private readonly IListingRepository _listingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly ILogger<DeleteListingHandler> _logger;

    public DeleteListingHandler(
        IListingRepository listingRepository,
        IUnitOfWork unitOfWork,
        IClock clock,
        ILogger<DeleteListingHandler> logger)
    {
        _listingRepository = listingRepository;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _logger = logger;
    }

    /// <summary>
    /// Disables a listing if the requester is the listing owner.
    /// / Desativa um anúncio se o solicitante for o dono do anúncio.
    /// </summary>
    public async Task<DeleteListingResult> HandleAsync(
        DeleteListingCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Starting delete listing flow. ListingId: {ListingId}, RequesterId: {RequesterId}",
            command.ListingId,
            command.RequesterId);

        if (command.ListingId == Guid.Empty)
        {
            _logger.LogWarning(
                "Delete listing failed because listing id is empty. RequesterId: {RequesterId}",
                command.RequesterId);

            throw new ArgumentException("LISTING_ID_REQUIRED");
        }

        if (command.RequesterId == Guid.Empty)
        {
            _logger.LogWarning(
                "Delete listing failed because requester id is empty. ListingId: {ListingId}",
                command.ListingId);

            throw new ArgumentException("REQUESTER_ID_REQUIRED");
        }

        var listing = await _listingRepository.GetByIdAsync(
            command.ListingId,
            cancellationToken);

        if (listing is null)
        {
            _logger.LogWarning(
                "Delete listing failed because listing was not found. ListingId: {ListingId}, RequesterId: {RequesterId}",
                command.ListingId,
                command.RequesterId);

            throw new ListingNotFoundException();
        }

        if (listing.OwnerId != command.RequesterId)
        {
            _logger.LogWarning(
                "Delete listing failed because requester is not the listing owner. ListingId: {ListingId}, OwnerId: {OwnerId}, RequesterId: {RequesterId}",
                listing.Id,
                listing.OwnerId,
                command.RequesterId);

            throw new InvalidListingOwnerException();
        }

        var nowUtc = _clock.UtcNow;

        listing.Delete(nowUtc);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Delete listing flow completed successfully. ListingId: {ListingId}, RequesterId: {RequesterId}, Status: {Status}",
                listing.Id,
                command.RequesterId,
                listing.Status);
        }
        catch(Exception ex)
        {
            throw new DatabaseSaveFailedException(ex);
        }
        
        return new DeleteListingResult(
            listing.Id,
            listing.Status,
            listing.UpdatedAtUtc);
    }
}