namespace UserAccess.Application.Auth.Logout.Records;

public record LogoutCurrentSessionCommand(
    string RefreshToken
    );