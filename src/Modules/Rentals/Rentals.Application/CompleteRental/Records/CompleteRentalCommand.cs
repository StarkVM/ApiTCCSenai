namespace Rentals.Application.CompleteRental.Records;

public sealed record CompleteRentalCommand(
    Guid RentalId,
    Guid RequesterId
);