namespace UserAccess.Application.Abstractions;

/// <summary>
/// Defines profile photo URL generation.
/// / Define a geração de URL para foto de perfil.
/// </summary>
public interface IUserProfilePhotoUrlProvider
{
    UserProfilePhotoAccessUrl Generate(string storageKey);
}

/// <summary>
/// Represents a temporary profile photo access URL.
/// / Representa uma URL temporária de acesso da foto de perfil.
/// </summary>
public sealed record UserProfilePhotoAccessUrl(
    string Url,
    DateTime ExpiresAtUtc
);