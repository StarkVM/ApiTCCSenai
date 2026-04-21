namespace UserAccess.Application.Auth.Logout.Records;

public record LogoutAllSessionsCommand(
    Guid UserId
    );