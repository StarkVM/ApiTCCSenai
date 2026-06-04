namespace UserAccess.Domain.Exceptions.UserAccessExceptions;

/// <summary>
/// Exception thrown when a non-active user tries to become a provider.
/// / Exceção lançada quando um usuário não ativo tenta se tornar provider.
/// </summary>
public sealed class UserMustBeActiveToBecomeProviderException : AppException
{
    public UserMustBeActiveToBecomeProviderException()
        : base(
            code: "INVALID_USER",
            message: "Only active users can become providers")
    {
    }
}

