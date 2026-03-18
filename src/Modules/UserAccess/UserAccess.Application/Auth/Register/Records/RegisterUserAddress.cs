namespace UserAccess.Application.Auth.Register.Records;

public record RegisterUserAddress(
    string State,
    string City,
    string District,
    string Street,
    string ZipCode
    );