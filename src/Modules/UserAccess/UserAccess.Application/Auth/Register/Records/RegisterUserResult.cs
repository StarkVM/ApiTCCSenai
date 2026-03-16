namespace UserAccess.Application.Auth.Register.Records;

public record RegisterUserResult(
    Guid UserId,
    string Email,
    DateTime CreatedAtUtc
    );