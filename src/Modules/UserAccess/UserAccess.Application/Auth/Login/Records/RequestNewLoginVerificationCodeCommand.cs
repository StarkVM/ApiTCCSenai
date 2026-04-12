namespace UserAccess.Application.Auth.Login.Records;

public record RequestNewLoginVerificationCodeCommand(
    string Email
    );