namespace Listings.Domain.Exceptions.ListingsExceptions;

public sealed class InvalidListingOwnerException : AppException
{
    public InvalidListingOwnerException()
        : base(
            code: "INVALID_LISTING_OWNER",
            message: "Invalid listing owner.")
    {
    }
    
}