

namespace UserAccess.Domain.Exceptions.UserAccessExceptions;

/// <summary>
/// Exception thrown when email verification cannot be completed.
/// / Exceção lançada quando a verificação de email não pode ser concluída.
/// </summary>
public sealed class EmailVerificationFailedException : AppException
{
    public EmailVerificationFailedException()
        : base(
            code: "EMAIL_VERIFICATION_FAILED",
            message: "Unable to verify email.")
    {
    }
}