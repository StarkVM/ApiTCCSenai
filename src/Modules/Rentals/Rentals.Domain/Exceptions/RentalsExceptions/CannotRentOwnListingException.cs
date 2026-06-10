namespace Rentals.Domain.Exceptions.RentalsExceptions;

public class CannotRentOwnListingException : AppException
{
    public CannotRentOwnListingException()
        : base(
            code: "CANNOT_RENT_OWN_LISTING",
            message: "Cannot rent own listing exception")
    {
    }
}