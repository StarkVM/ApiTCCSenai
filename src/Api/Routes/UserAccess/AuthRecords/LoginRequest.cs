namespace Api.Routes.UserAccess.AuthRecords;

public record LoginRequest(
    string Email,
    string Password);