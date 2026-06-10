namespace Listings.Domain.Exceptions.ListingsExceptions;

public class ListingCannotBeEditedException : AppException
{
    public ListingCannotBeEditedException(string code, string message)
        : base(
            code: code,
            message: message)
    {
    }
}