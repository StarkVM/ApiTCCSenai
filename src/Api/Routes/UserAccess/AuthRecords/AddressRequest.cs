namespace Api.Routes.UserAccess.AuthRecords;

public record AddressRequest(
    string State,
    string City,
    string District,
    string Street,
    string ZipCode
    );