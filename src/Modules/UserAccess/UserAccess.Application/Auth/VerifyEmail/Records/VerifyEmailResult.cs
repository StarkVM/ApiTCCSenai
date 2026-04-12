namespace UserAccess.Application.Auth.VerifyEmail.Records;

public record VerifyEmailResult(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAtUtc,
    DateTime RefreshTokenExpiresAtUtc
    );