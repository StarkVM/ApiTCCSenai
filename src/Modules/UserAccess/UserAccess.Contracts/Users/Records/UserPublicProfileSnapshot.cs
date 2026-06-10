namespace UserAccess.Contracts.Users.Records;

/// <summary>
/// Represents public user information exposed to other modules.
/// / Representa informações públicas do usuário expostas para outros módulos.
/// </summary>
public sealed record UserPublicProfileSnapshot(
    Guid UserId,
    string FirstName,
    string LastName,
    bool IsActive)
{
    public string FullName => $"{FirstName} {LastName}".Trim();
}