
namespace UserAccess.Domain.Exceptions.UserAccessExceptions;

/// <summary>
/// Exception thrown when both email and CPF are already registered.
/// / Exceção lançada quando email e CPF já estão cadastrados.
/// </summary>
public sealed class EmailAndCpfConflictException : AppException
{
    public EmailAndCpfConflictException()
        : base(
            code: "EMAIL_AND_CPF_CONFLICT",
            message: "Email and CPF already registered.")
    {
    }
}