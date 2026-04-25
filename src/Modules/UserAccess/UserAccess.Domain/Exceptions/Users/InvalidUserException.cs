
namespace UserAccess.Domain.Exceptions.Users;

/// <summary>
/// Exception thrown when the user is invalid.
/// / Exceção lançada quando o usuário é inválido.
/// </summary>
public sealed class UsersInvalidUserException : AppException
{
    public UsersInvalidUserException()
        : base(
            code: "INVALID_USER",
            message: "User is invalid.")
    {
    }
}