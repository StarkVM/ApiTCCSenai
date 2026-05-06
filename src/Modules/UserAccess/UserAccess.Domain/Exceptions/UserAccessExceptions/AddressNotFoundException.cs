namespace UserAccess.Domain.Exceptions.UserAccessExceptions;


/// <summary>
/// Exception thrown when the user's address is not found.
/// / Exceção lançada quando o endereço do usuário não é encontrado.
/// </summary>
public sealed class AddressNotFoundException : AppException
{
    public AddressNotFoundException()
        : base(
            code: "ADDRESS_NOT_FOUND",
            message: "Address not found")
    {
    }
}