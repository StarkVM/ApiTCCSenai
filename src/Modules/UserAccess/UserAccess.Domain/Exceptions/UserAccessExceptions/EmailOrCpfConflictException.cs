

namespace UserAccess.Domain.Exceptions.UserAccessExceptions;

/// <summary>
/// Exception thrown when email or CPF is already registered.
/// / Exceção lançada quando email ou CPF já estão cadastrados.
/// </summary>
public sealed class EmailOrCpfConflictException : AppException
{
    public EmailOrCpfConflictException()
        : base(
            code: "EMAIL_OR_CPF_CONFLICT",
            message: "Email or CPF already registered.")
    {
    }
}