namespace UserAccess.Domain.Exceptions.Users;

/// <summary>
/// Exception thrown when the user id is invalid.
/// / Exceção lançada quando o identificador do usuário é inválido.
/// </summary>
public sealed class UsersInvalidUserIdException : AppException
{
    public UsersInvalidUserIdException()
        : base(
            code: "INVALID_USER_ID",
            message: "User id is not valid.")
    {
    }
}