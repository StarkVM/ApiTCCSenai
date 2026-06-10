namespace UserAccess.Application.ProfilePhotos.Records;

public sealed record ProfilePhotoResult(
    Guid UserId,
    bool HasPhoto,
    string? Url,
    DateTime? UrlExpiresAtUtc,
    DateTime? UpdatedAtUtc
);