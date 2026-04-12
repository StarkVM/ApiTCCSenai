namespace Api.Routes.UserAccess.Records;

public record VerifyLoginRequest(
    string Email,
    string Code
    );