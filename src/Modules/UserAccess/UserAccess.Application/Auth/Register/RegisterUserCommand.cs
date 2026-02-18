namespace UserAccess.Application.Auth.Register;

public record RegisterUserCommand(
    string FristName,
    string LastName,
    DateTime BirthDate,
    string Email
    );