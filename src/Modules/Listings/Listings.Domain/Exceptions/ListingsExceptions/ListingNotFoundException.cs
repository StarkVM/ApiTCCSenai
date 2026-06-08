namespace Listings.Domain.Exceptions.ListingsExceptions;

public class ListingNotFoundException : AppException
{
    public ListingNotFoundException()
        : base(
        code: "LISTING_NOT_FOUND",
        message: "Listing not found.")
    {
    }
}