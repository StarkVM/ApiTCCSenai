namespace UserAccess.Domain.Senders;

public record CreateProviderIdentityVerificationSessionRequest(
    Guid LocalSessionId,
    string FirstName,
    string LastName,
    DateOnly BirthDate,
    string Email
    );