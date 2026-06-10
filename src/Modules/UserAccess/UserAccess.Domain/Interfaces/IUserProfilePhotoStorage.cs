using UserAccess.Domain.Files;

namespace UserAccess.Domain.Interfaces;

/// <summary>
/// Defines user profile photo storage operations.
/// / Define operações de armazenamento de foto de perfil do usuário.
/// </summary>
public interface IUserProfilePhotoStorage
{
    Task<string> UploadAsync(
        Guid userId,
        UserProfilePhotoUpload photo,
        CancellationToken cancellationToken);
    
    Task DeleteAsync(
        string storageKey,
        CancellationToken cancellationToken);
}