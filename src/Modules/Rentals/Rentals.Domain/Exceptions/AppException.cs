namespace Rentals.Domain.Exceptions;

/// <summary>
/// Base exception for known application errors.
/// / Exceção base para erros conhecidos da aplicação.
/// </summary>
public class AppException : Exception
{
    /// <summary>
    /// Error code used by clients (frontend).
    /// / Código do erro usado pelo cliente (frontend).
    /// </summary>
    public string Code { get; }

    public AppException(string code, string message) : base(message)
    {
        Code = code;
    }
}