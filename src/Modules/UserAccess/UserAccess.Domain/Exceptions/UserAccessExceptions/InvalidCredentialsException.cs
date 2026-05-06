
namespace UserAccess.Domain.Exceptions.UserAccessExceptions;

/// <summary>
/// Exception thrown when the provided credentials are invalid.
/// / Exceção lançada quando as credenciais informadas são inválidas.
/// </summary>
public sealed class InvalidCredentialsException : AppException
{
    public InvalidCredentialsException()
        : base(
            code: "INVALID_CREDENTIALS",
            message: "Invalid credentials.")
    {
    }
}