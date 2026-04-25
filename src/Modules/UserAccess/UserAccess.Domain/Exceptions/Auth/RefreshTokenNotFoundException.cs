

namespace UserAccess.Domain.Exceptions.Auth;

/// <summary>
/// Exception thrown when the refresh token is not found.
/// / Exceção lançada quando o refresh token não é encontrado.
/// </summary>
public sealed class RefreshTokenNotFoundException : AppException
{
    public RefreshTokenNotFoundException()
        : base(
            code: "REFRESH_TOKEN_NOT_FOUND",
            message: "Refresh token not found.")
    {
    }
}