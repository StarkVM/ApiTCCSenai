namespace Rentals.Application.CreateRental.Records;

public record CreateRentalCommand(
    Guid ListingId,
    Guid RenterId,
    DateOnly StartDate,
    DateOnly EndDate,
    bool IncludeOperator,
    bool IncludeFreight
    );