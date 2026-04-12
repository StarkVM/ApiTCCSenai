namespace Api.Routes.UserAccess.Records;

public record VerifyEmailRequest(
    string Email,
    string Code
    );