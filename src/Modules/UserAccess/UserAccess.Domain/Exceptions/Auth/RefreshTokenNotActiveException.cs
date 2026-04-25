
namespace UserAccess.Domain.Exceptions.Auth;

/// <summary>
/// Exception thrown when the refresh token is not active.
/// / Exceção lançada quando o refresh token não está ativo.
/// </summary>
public sealed class RefreshTokenNotActiveException : AppException
{
    public RefreshTokenNotActiveException()
        : base(
            code: "REFRESH_TOKEN_NOT_ACTIVE",
            message: "Refresh token not active.")
    {
    }
}