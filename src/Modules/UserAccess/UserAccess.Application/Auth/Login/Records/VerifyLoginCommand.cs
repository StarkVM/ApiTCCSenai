namespace UserAccess.Application.Auth.Login.Records;

public record VerifyLoginCommand(
    string Email,
    string Code
    );