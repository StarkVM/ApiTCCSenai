namespace UserAccess.Application.Auth.Register.Records;

public record RegisterUserCommand(
    string FirstName,
    string LastName,
    DateTime BirthDate,
    string Email,
    string Cpf,
    string Password
    );