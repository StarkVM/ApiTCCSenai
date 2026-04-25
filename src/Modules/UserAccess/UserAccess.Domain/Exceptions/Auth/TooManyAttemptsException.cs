
namespace UserAccess.Domain.Exceptions.Auth;

/// <summary>
/// Exception thrown when too many attempts are made in a short time.
/// / Exceção lançada quando muitas tentativas são feitas em pouco tempo.
/// </summary>
public sealed class TooManyAttemptsException : AppException
{
    public TooManyAttemptsException()
        : base(
            code: "TOO_MANY_ATTEMPTS",
            message: "Too many attempts. Please try again later.")
    {
    }
}