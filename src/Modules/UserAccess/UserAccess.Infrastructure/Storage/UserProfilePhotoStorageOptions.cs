namespace UserAccess.Infrastructure.Storage;

/// <summary>
/// Options used by user profile photo storage.
/// / Opções utilizadas pelo armazenamento de foto de perfil.
/// </summary>
public sealed class UserProfilePhotoStorageOptions
{
    public string BucketName { get; init; } = string.Empty;

    public string BasePrefix { get; init; } = "users";

    public int ReadUrlExpirationMinutes { get; init; } = 60;
}