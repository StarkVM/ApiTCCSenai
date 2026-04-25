

namespace UserAccess.Domain.Exceptions.Auth;

/// <summary>
/// Exception thrown when a refresh token is required but was not provided.
/// / Exceção lançada quando o refresh token é obrigatório e não foi informado.
/// </summary>
public sealed class RefreshTokenRequiredException : AppException
{
    public RefreshTokenRequiredException()
        : base(
            code: "REFRESH_TOKEN_REQUIRED",
            message: "Refresh token is required.")
    {
    }
}