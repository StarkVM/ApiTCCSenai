namespace UserAccess.Application.Auth.Login.Records;

public record LoginCommand(
    string Email,
    string Password
    );