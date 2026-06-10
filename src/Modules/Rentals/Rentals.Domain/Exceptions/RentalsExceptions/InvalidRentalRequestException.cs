namespace Rentals.Domain.Exceptions.RentalsExceptions;

public class InvalidRentalRequestException: AppException
{
    public InvalidRentalRequestException(string message)
        : base(
        code: "INVALID_RENTAL_REQUEST",
        message: message)
    {
    }
}