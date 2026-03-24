namespace UserAccess.Application.Auth.VerifyEmail.Records;

public record ResquestEmailVerificationCommand(
    string Email,
    string Code
    );