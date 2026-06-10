namespace Rentals.Domain.Exceptions.RentalsExceptions;

public class ListingAlreadyHasActiveRentalException : AppException
{
    public ListingAlreadyHasActiveRentalException()
        : base(
            code: "LISTING_ALREADY_HAS_ACTIVE_RENTAL",
            message: "Listing has active rental.")
    {
    }
}