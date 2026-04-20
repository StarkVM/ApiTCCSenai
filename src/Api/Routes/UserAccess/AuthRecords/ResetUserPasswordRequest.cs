namespace Api.Routes.UserAccess.AuthRecords;

public record ResetUserPasswordRequest(
    string Email,
    string NewPassword,
    string Code
    );