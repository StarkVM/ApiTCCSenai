namespace Api.Routes.UserAccess.Records;

public record LoginRequest(
    string Email,
    string Password);