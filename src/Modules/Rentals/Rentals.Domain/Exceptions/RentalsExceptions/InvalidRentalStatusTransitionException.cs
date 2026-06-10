namespace Rentals.Domain.Exceptions.RentalsExceptions;

public class InvalidRentalStatusTransitionException : AppException
{
    public InvalidRentalStatusTransitionException(string message)
        : base(
            code: "INVALID_RENTAL_STATUS_TRANSITION",
            message: message)
    {
    }
}