namespace Api.Routes.UserAccess.AuthRecords;

public record VerifyEmailResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpirationAtUtc,
    DateTime RefreshTokenExpirationAtUtc);