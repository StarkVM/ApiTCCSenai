namespace UserAccess.Domain.Files;

/// <summary>
/// Represents an uploaded user profile photo without depending on ASP.NET types.
/// / Representa uma foto de perfil enviada sem depender de tipos do ASP.NET.
/// </summary>
public sealed record UserProfilePhotoUpload(
    string FileName,
    string ContentType,
    long Length,
    Func<Stream> OpenReadStream
);