namespace Rentals.Application.GetRentalById.Records;

public sealed record GetRentalByIdQuery(
    Guid RentalId,
    Guid RequesterId
);