namespace Api.Routes.UserAccess.Records;

public record ResetUserPasswordRequest(
    string Email,
    string NewPassword,
    string Code
    );