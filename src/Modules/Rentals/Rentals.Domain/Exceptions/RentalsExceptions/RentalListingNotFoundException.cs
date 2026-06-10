namespace Rentals.Domain.Exceptions.RentalsExceptions;

public class RentalListingNotFoundException : AppException
{
    public RentalListingNotFoundException()
        : base(
            code: "LISTING_NOT_FOUND",
            message: "Listing not found.")
    {
    }
}