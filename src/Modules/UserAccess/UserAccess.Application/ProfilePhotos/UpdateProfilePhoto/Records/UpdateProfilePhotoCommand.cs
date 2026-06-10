using UserAccess.Domain.Files;

namespace UserAccess.Application.ProfilePhotos.UpdateProfilePhoto.Records;

public sealed record UpdateProfilePhotoCommand(
    Guid UserId,
    UserProfilePhotoUpload Photo
);