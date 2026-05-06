

namespace UserAccess.Domain.Exceptions.UserAccessExceptions;

/// <summary>
/// Exception thrown when the user id is invalid.
/// / Exceção lançada quando o identificador do usuário é inválido.
/// </summary>
public sealed class InvalidUserIdException : AppException
{
    public InvalidUserIdException()
        : base(
            code: "INVALID_USER_ID",
            message: "User id is not valid.")
    {
    }
}