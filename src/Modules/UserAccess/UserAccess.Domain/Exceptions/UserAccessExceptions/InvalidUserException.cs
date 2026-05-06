

namespace UserAccess.Domain.Exceptions.UserAccessExceptions;

/// <summary>
/// Exception thrown when the user is invalid.
/// / Exceção lançada quando o usuário é inválido.
/// </summary>
public sealed class InvalidUserException : AppException
{
    public InvalidUserException()
        : base(
            code: "INVALID_USER",
            message: "User is invalid.")
    {
    }
}