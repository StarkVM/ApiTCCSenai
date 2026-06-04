namespace UserAccess.Domain.Exceptions.UserAccessExceptions;

/// <summary>
/// Exception thrown when a user without verified email tries to delete the account.
/// / Exceção lançada quando um usuário sem email verificado tenta deletar a conta.
/// </summary>
public sealed class UserEmailMustBeVerifiedToDeleteException : AppException
{
    public UserEmailMustBeVerifiedToDeleteException()
        : base(
            code: "INVALID_USER",
            message: "Only users with verified email can delete their account.")
    {
    }
}
    