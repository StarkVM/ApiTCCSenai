namespace UserAccess.Application.Auth.Login.Records;

public record VerifyLoginResult(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAtUtc,
    DateTime RefreshTokenExpiresAtUtc);