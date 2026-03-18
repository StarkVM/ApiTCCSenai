namespace Api.Routes.UserAccess.Records;

public record AddressRequest(
    string State,
    string City,
    string District,
    string Street,
    string ZipCode
    );