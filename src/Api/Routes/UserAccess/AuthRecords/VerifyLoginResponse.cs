namespace Api.Routes.UserAccess.AuthRecords;

public record VerifyLoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAtUtc,
    DateTime RefreshTokenExpiresAtUtc
    );