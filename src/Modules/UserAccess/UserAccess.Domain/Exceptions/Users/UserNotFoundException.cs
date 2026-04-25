
namespace UserAccess.Domain.Exceptions.Users;

/// <summary>
/// Exception thrown when the user is not found.
/// / Exceção lançada quando o usuário não é encontrado.
/// </summary>
public sealed class UserNotFoundException : AppException
{
    public UserNotFoundException()
        : base(
            code: "USER_NOT_FOUND",
            message: "User not found.")
    {
    }
}