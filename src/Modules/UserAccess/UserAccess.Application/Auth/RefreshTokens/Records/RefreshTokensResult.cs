namespace UserAccess.Application.Auth.RefreshTokens.Records;

public record RefreshTokensResult(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAtUtc,
    DateTime RefreshTokenExpiresAtUtc
    );