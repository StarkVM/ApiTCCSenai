namespace Api.Routes.UserAccess.AuthRecords;

public record LogoutCurrentSessionRequest(
    string RefreshToken
    );