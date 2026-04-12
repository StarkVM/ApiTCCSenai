namespace Api.Routes.UserAccess.Records;

public record VerifyLoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAtUtc,
    DateTime RefreshTokenExpiresAtUtc
    );