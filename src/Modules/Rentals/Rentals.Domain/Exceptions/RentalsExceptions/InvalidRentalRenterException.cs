namespace Rentals.Domain.Exceptions.RentalsExceptions;

public class InvalidRentalRenterException : AppException
{
    public InvalidRentalRenterException(string message)
        : base(
            code: "INVALID_RENTAL_RENTER",
            message: message)
    {
    }
}