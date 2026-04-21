namespace Api.Routes.UserAccess.AuthRecords;

public sealed record RegisterUserRequest(
    string FirstName,
    string LastName,
    DateOnly BirthDate,
    string Email,
    string Cpf,
    string Password,
    AddressRequest Address
    );