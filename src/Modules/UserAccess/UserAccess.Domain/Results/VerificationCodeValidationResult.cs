

using UserAccess.Domain.Entities;

namespace UserAccess.Domain.Results;


/// <summary>
/// Resultado da validação do código.
/// Result of verification code validation.
/// </summary>
public class VerificationCodeValidationResult
{
    public bool IsValid { get; }
    public string? Error { get; }
    public EmailVerificationCode? Code { get; }

    private VerificationCodeValidationResult(bool isValid, string? error, EmailVerificationCode? code)
    {
        IsValid = isValid;
        Error = error;
        Code = code;
    }

    public static VerificationCodeValidationResult Success(EmailVerificationCode code)
    => new (true, null, code);
    
    public static VerificationCodeValidationResult Failure(string error)
    => new  (false, error, null);
    
}