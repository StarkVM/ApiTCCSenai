namespace UserAccess.Application.Auth.Register;

public record RegisterUserCommand(
    string FirstName,
    string LastName,
    DateTime BirthDate,
    string Email,
    string Cpf,
    string Password
    );