namespace UserAccess.Application.Auth.Common.Records;

public record AuthTokensResult(
    string AccessToken,
    string RefreshToken,
    string RefreshTokenHash,
    DateTime AccessTokenExpiresAtUtc,
    DateTime RefreshTokenExpiresAtUtc
    );