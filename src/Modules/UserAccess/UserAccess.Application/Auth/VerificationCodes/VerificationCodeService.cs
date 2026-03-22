using UserAccess.Domain.Entities;
using UserAccess.Domain.Enums;
using UserAccess.Domain.Interfaces;
using UserAccess.Domain.Results;

namespace UserAccess.Application.Auth.VerificationCodes;

public sealed class VerificationCodeService : IVerificationCodeService
{
    private readonly IVerificationCodeRepository _verificationCodeRepository;
    private readonly IVerificationCodeHasher _verificationCodeHasher;
    private readonly IClock _clock;

    public VerificationCodeService(
        IVerificationCodeRepository verificationCodeRepository,
        IVerificationCodeHasher verificationCodeHasher,
        IClock clock)
    {
        _verificationCodeRepository = verificationCodeRepository;
        _verificationCodeHasher = verificationCodeHasher;
        _clock = clock;
    }
    public async Task<VerificationCodeValidationResult> ValidateAsync(
        User user,
        string code,
        VerificationCodePurpose purpose,
        CancellationToken cancellationToken
    )
    {
        // Busca o código mais recente para aquele usuário e propósito
        // Get latest code for user and purpose
        var utcNow = _clock.UtcNow;
        var verificationCode =
            await _verificationCodeRepository.GetLatestActiveAsync(user.Id, purpose, cancellationToken);

        if (verificationCode is null)
        {
            return VerificationCodeValidationResult.Faliure("CODE_NOT_FOUND");
        }
        
        // Verifica se já foi usado
        // Check if already used
        if (verificationCode.IsConsumed())
        {
            return VerificationCodeValidationResult.Faliure("CODE_ALREADY_USED");
        }
        
        // Verifica expiração
        // Check expiration
        if (verificationCode.IsExpired(utcNow))
        {
            return VerificationCodeValidationResult.Faliure("CODE_EXPIRED");
        }
        
        // Valida hash
        // Validate hash
        var isValid = _verificationCodeHasher.Verify(code, verificationCode.CodeHash);

        if (!isValid)
        {
            return VerificationCodeValidationResult.Faliure("CODE_INVALID");
        }
        
        // Sucesso
        // Success
        
        return VerificationCodeValidationResult.Sucess(verificationCode);
    }
}