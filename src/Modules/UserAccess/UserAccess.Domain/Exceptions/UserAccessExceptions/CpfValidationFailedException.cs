namespace UserAccess.Domain.Exceptions.UserAccessExceptions;

/// <summary>
/// Exception thrown when user identity verification fails.
/// / Exceção lançada quando a verificação de identidade do usuário falha.
/// </summary>
public sealed class CpfValidationFailedException : AppException
{
    public CpfValidationFailedException()
        : base(
            code: "USER_CPF_VALIDATION_FAILED",
            message: "User cpf validation failed.")
    {
    }
}