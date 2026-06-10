namespace Rentals.Domain.Exceptions.RentalsExceptions;

public class ListingUnavailableForRentalException : AppException
{
    public ListingUnavailableForRentalException(string message)
        : base(
            code: "LISTING_UNAVAILABLE_FOR_RENTAL",
            message: message)
    {
    }
}