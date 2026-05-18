namespace UserAccess.Domain.Senders;

public record VerifyProviderIdentityRequest(
    string ProviderSessionId,
    string ExpectedFirstName,
    string ExpectedLastName,
    DateOnly ExpectedBirthDate,
    string ExpectedCpfHash
    );