namespace UserAccess.Application.Auth.ResetPassword.Records;

public record ResetPasswordCommand(
    string Email,
    string NewPassword,
    string Code
    );