using UserAccess.Domain.Entities;

namespace Api.Routes.UserAccess.Records;

public sealed record RegisterRequest(
    string FirstName,
    string LastName,
    DateTime BirthDate,
    string Email,
    string Cpf,
    string Password,
    AddressRequest Address
    );