namespace Api.Routes.UserAccess.Records;

public record VerifyEmailResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpirationAtUtc,
    DateTime RefreshTokenExpirationAtUtc);