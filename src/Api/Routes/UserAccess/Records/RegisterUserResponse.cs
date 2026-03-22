namespace Api.Routes.UserAccess.Records;

public sealed record RegisterUserResponse(
    Guid UserId,
    string Email,
    DateTime CreatedAtUtc
    );