namespace Api.Routes.UserAccess.AuthRecords;

public sealed record RegisterUserResponse(
    Guid UserId,
    string Email,
    DateTime CreatedAtUtc
    );