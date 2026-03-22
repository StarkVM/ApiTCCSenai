using UserAccess.Domain.Entities;
using UserAccess.Domain.Enums;
using UserAccess.Domain.Results;

namespace UserAccess.Domain.Interfaces;

public interface IVerificationCodeService
{
        Task<VerificationCodeValidationResult> ValidateAsync(
        User user,
        string code,
        VerificationCodePurpose purpose,
        CancellationToken cancellationToken
        );
}