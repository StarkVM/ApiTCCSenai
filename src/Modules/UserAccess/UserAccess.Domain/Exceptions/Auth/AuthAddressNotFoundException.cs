namespace UserAccess.Domain.Exceptions.Auth;


/// <summary>
/// Exception thrown when the user's address is not found.
/// / Exceção lançada quando o endereço do usuário não é encontrado.
/// </summary>
public sealed class AuthAddressNotFoundException : AppException
{
    public AuthAddressNotFoundException()
        : base(
            code: "ADDRESS_NOT_FOUND",
            message: "Address not found")
    {
    }
}