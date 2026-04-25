

namespace UserAccess.Domain.Exceptions.Auth;

/// <summary>
/// Exception thrown when the user id is invalid.
/// / Exceção lançada quando o identificador do usuário é inválido.
/// </summary>
public sealed class AuthInvalidUserIdException : AppException
{
    public AuthInvalidUserIdException()
        : base(
            code: "INVALID_USER_ID",
            message: "User id is not valid.")
    {
    }
}