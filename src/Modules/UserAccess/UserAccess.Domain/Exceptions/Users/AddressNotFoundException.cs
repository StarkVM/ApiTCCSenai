
namespace UserAccess.Domain.Exceptions.Users;


/// <summary>
/// Exception thrown when the user's address is not found.
/// / Exceção lançada quando o endereço do usuário não é encontrado.
/// </summary>
public sealed class UsersAddressNotFoundException : AppException
{
    public UsersAddressNotFoundException()
        : base(
            code: "ADDRESS_NOT_FOUND",
            message: "Address not found")
    {
    }
}