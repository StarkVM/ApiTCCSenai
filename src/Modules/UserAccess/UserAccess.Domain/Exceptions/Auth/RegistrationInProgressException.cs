
namespace UserAccess.Domain.Exceptions.Auth;

/// <summary>
/// Exception thrown when a registration is already in progress.
/// / Exceção lançada quando já existe um cadastro em andamento.
/// </summary>
public sealed class RegistrationInProgressException : AppException
{
    public RegistrationInProgressException()
        : base(
            code: "REGISTRATION_IN_PROGRESS",
            message: "Registration already in progress.")
    {
    }
}