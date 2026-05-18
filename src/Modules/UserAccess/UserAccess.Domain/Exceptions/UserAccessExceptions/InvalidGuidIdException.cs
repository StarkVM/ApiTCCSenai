namespace UserAccess.Domain.Exceptions.UserAccessExceptions;

/// <summary>
/// Exception thrown when the Guid id is invalid.
/// / Exceção lançada quando o identificador Guid eh invalido.
/// </summary>
public sealed class InvalidGuidIdException : AppException
{
    public InvalidGuidIdException()
        : base(
            code: "INVALID_USER_ID",
            message: "User id is not valid.")
    {
    }
}