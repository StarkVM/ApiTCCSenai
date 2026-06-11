namespace UserAccess.Contracts.Users.Records;

/// <summary>
/// Represents public user profile data with profile photo information.
/// / Representa dados públicos do usuário com informações da foto de perfil.
/// </summary>
public sealed record UserPublicProfileWithPhotoSnapshot(
    Guid UserId,
    string FirstName,
    string LastName,
    string? ProfilePhotoUrl,
    DateTime? ProfilePhotoUrlExpiresAtUtc)
{
    /// <summary>
    /// User's full public name.
    /// / Nome público completo do usuário.
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();
}