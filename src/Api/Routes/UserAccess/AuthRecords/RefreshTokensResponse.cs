namespace Api.Routes.UserAccess.AuthRecords;

public record RefreshTokensResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpirationAtUtc,
    DateTime RefreshTokenExpirationAtUtc
    );