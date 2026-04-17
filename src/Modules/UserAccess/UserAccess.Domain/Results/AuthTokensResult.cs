namespace UserAccess.Domain.Results;

public record AuthTokensResult(
    string AccessToken,
    string RefreshToken,
    string RefreshTokenHash,
    DateTime AccessTokenExpiresAtUtc,
    DateTime RefreshTokenExpiresAtUtc
    );