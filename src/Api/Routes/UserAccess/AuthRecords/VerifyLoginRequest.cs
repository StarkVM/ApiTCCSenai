namespace Api.Routes.UserAccess.AuthRecords;

public record VerifyLoginRequest(
    string Email,
    string Code
    );