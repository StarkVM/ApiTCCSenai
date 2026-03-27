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
    private readonly IUnitOfWork _unitOfWork;

    public VerificationCodeService(
        IVerificationCodeRepository verificationCodeRepository,
        IVerificationCodeHasher verificationCodeHasher,
        IClock clock,
        IUnitOfWork unitOfWork)
    {
        _verificationCodeRepository = verificationCodeRepository;
        _verificationCodeHasher = verificationCodeHasher;
        _clock = clock;
        _unitOfWork = unitOfWork;
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

        var independentCode = await _verificationCodeRepository.GetLatestAsync(user.Id, purpose, cancellationToken);
        
        var verificationCode = await _verificationCodeRepository.GetLatestActiveAsync(user.Id, purpose, cancellationToken);
        
        if (verificationCode is null)
        {
            return VerificationCodeValidationResult.Failure("VALID_CODE_NOT_FOUND");
        }
        
        if (verificationCode.Attempts >= 5)
        {
            verificationCode.Consume(utcNow);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return VerificationCodeValidationResult.Failure("TOO_MANY_ATTEMPTS");
        }
        
        // Valida hash
        // Validate hash
        var isValid = _verificationCodeHasher.Verify(code, verificationCode.CodeHash);

        if (!isValid)
        {
            verificationCode.IncrementAttempts();
            if (verificationCode.Attempts >= 5)
            {
                verificationCode.Consume(utcNow);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return VerificationCodeValidationResult.Failure("TOO_MANY_ATTEMPTS");
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return VerificationCodeValidationResult.Failure("CODE_INVALID");
        }
        
        // Sucesso
        // Success
        return VerificationCodeValidationResult.Success(verificationCode);
    }
}