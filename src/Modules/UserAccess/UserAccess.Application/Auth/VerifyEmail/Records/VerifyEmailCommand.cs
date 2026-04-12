namespace UserAccess.Application.Auth.VerifyEmail.Records;

public record VerifyEmailCommand(
    string Email,
    string Code
    );