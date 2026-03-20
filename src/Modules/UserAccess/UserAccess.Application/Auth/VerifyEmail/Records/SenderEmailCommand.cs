using UserAccess.Domain.Enums;

namespace UserAccess.Application.Auth.VerifyEmail.Records;

public record SenderEmailCommand(
    string Email,
    Guid UserId,
    VerificationCodePurpose Purpose
    );