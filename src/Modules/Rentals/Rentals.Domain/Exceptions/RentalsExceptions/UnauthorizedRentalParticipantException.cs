namespace Rentals.Domain.Exceptions.RentalsExceptions;

public class UnauthorizedRentalParticipantException : AppException
{
    public UnauthorizedRentalParticipantException()
        : base(
            code: "UNAUTHORIZED_RENTAL_PARTICIPANT",
            message: "Unauthorized rental participant.")
    {
    }
}