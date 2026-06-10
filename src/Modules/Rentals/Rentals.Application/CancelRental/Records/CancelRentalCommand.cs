namespace Rentals.Application.CancelRental.Records;

public sealed record CancelRentalCommand(
    Guid RentalId,
    Guid RequesterId
);