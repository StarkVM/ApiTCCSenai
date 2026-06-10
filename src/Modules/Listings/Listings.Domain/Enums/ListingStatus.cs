namespace Listings.Domain.Enums;

public enum ListingStatus
{
    /// <summary>
    /// Listing was created and is waiting for review.
    /// / Anúncio foi criado e está aguardando análise.
    /// </summary>
    PendingReview = 0,

    /// <summary>
    /// Listing was reviewed and approved.
    /// / Anúncio foi analisado e aprovado.
    /// </summary>
    Approved = 1,

    /// <summary>
    /// Listing was reviewed and rejected.
    /// / Anúncio foi analisado e recusado.
    /// </summary>
    Rejected = 2,

    /// <summary>
    /// Listing was disabled by the system or moderation.
    /// / Anúncio foi desativado pelo sistema ou pela moderação.
    /// </summary>
    Suspended = 3,
    
    
    Deleted  = 4
}