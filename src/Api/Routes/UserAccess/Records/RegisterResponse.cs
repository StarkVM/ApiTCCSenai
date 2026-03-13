namespace Api.Routes.UserAccess.Records;

public sealed record RegisterResponse(
    Guid UserId,
    string Email,
    DateTime CreatedAtUtc
    );