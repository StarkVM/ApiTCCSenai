namespace Listings.Domain.Exceptions.ListingsExceptions;

public class InvalidListingImagesException : AppException
{
    public InvalidListingImagesException(string message)
        : base(
            code: "INVALID_LISTING_IMAGES",
            message: message)
    {
    }
}