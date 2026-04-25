

namespace UserAccess.Domain.Exceptions.Users;

/// <summary>
/// Exception thrown when the user is invalid.
/// / Exceção lançada quando o usuário é inválido.
/// </summary>
public sealed class AuthInvalidUserException : AppException
{
    public AuthInvalidUserException()
        : base(
            code: "INVALID_USER",
            message: "User is invalid.")
    {
    }
}