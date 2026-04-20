namespace Api.Routes.UserAccess.AuthRecords;

public record VerifyEmailRequest(
    string Email,
    string Code
    );