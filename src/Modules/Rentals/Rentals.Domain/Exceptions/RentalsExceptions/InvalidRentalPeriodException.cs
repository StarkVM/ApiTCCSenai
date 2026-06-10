namespace Rentals.Domain.Exceptions.RentalsExceptions;

public class InvalidRentalPeriodException : AppException
{
    public InvalidRentalPeriodException(string message)
        : base(
            code: "INVALID_RENTAL_PERIOD",
            message: message)
    {
    }
}