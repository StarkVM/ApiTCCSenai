namespace Api.Routes.UserAccess.Records;

public record RefreshTokensResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpirationAtUtc,
    DateTime RefreshTokenExpirationAtUtc
    );