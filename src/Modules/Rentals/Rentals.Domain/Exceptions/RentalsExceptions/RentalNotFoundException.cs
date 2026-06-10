namespace Rentals.Domain.Exceptions.RentalsExceptions;

public class RentalNotFoundException : AppException
{
    public RentalNotFoundException()
        : base(
            code: "RENTAL_NOT_FOUND",
            message: "Rental not found.")
    {
    }
}