namespace UserAccess.Application.Auth.Register;

public record RegisterUserResult(
    Guid UserId,
    string Email,
    DateTime CreatedAtUtc
    );