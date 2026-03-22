using UserAccess.Domain.Enums;

namespace UserAccess.Domain.Senders;

public record SendVerificationCodeRequest(
    string Email,
    Guid UserId,
    VerificationCodePurpose Purpose
);