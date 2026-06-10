namespace Api.Routes.Rentals.Records;

public record CreateRentalRequest(
    Guid ListingId,
    DateOnly StartDate,
    DateOnly EndDate,
    bool IncludeOperator,
    bool IncludeFreight
    );