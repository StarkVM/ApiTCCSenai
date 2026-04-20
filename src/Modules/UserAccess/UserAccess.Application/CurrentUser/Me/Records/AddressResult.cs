namespace UserAccess.Application.CurrentUser.Me.Records;

public record AddressResult(
    string State,
    string City,
    string District,
    string Street,
    string ZipCode
    );