namespace Rentals.Domain.Exceptions.RentalsExceptions;

public class RentalOptionUnavailableException : AppException
{
    public RentalOptionUnavailableException(string message)
        : base(
            code: "RENTAL_OPTION_UNAVAILABLE",
            message: message)
    {
    }
}