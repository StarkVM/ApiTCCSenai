using Listings.Domain.Enums;

namespace Listings.Domain.Entities;

public class Listing
{
     private const int MaxImages = 5;

    private readonly List<ListingImage> _images = [];


    public Guid Id { get; private set; }


    public Guid OwnerId { get; private set; }


    public string Title { get; private set; } = default!;


    public string Description { get; private set; } = default!;


    public decimal DailyPrice { get; private set; }


    public ListingStatus Status { get; private set; } = ListingStatus.PendingReview;


    public DateTime CreatedAtUtc { get; private set; }


    public DateTime UpdatedAtUtc { get; private set; }

    public DateTime? ReviewedAtUtc { get; private set; }

    
    public string? RejectionReason { get; private set; }


    public IReadOnlyCollection<ListingImage> Images => _images.AsReadOnly();

    private Listing()
    {
    }

    public Listing(
        Guid id,
        Guid ownerId,
        string title,
        string description,
        decimal dailyPrice,
        DateTime createdAtUtc)
    {
        if (ownerId == Guid.Empty)
        {
            throw new ArgumentException("Owner id cannot be empty.");
        }

        if (dailyPrice <= 0)
        {
            throw new ArgumentException("Daily price must be greater than zero.");
        }

        Id = id;
        OwnerId = ownerId;
        Title = title.Trim();
        Description = description.Trim();
        DailyPrice = dailyPrice;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
        Status = ListingStatus.PendingReview;
    }

    /// <summary>
    /// Adds a new image to the listing while respecting the maximum limit.
    /// / Adiciona uma nova imagem ao anúncio respeitando o limite máximo.
    /// </summary>
    public void AddImage(
        Guid imageId,
        string storageKey,
        int displayOrder,
        DateTime createdAtUtc)
    {
        if (_images.Count >= MaxImages)
        {
            throw new InvalidOperationException("A listing cannot contain more than five images.");
        }

        if (displayOrder < 1 || displayOrder > MaxImages)
        {
            throw new ArgumentException("Display order must be between 1 and 5.");
        }

        if (_images.Any(image => image.DisplayOrder == displayOrder))
        {
            throw new InvalidOperationException("This display order is already being used.");
        }

        var image = new ListingImage(
            imageId,
            Id,
            storageKey,
            displayOrder,
            createdAtUtc);

        _images.Add(image);
        UpdatedAtUtc = createdAtUtc;
    }

    /// <summary>
    /// Approves the listing after moderation.
    /// / Aprova o anúncio após a moderação.
    /// </summary>
    public void Approve(DateTime reviewedAtUtc)
    {
        if (Status != ListingStatus.PendingReview)
        {
            throw new InvalidOperationException("Only pending review listings can be approved.");
        }

        Status = ListingStatus.Approved;
        ReviewedAtUtc = reviewedAtUtc;
        RejectionReason = null;
        UpdatedAtUtc = reviewedAtUtc;
    }

    /// <summary>
    /// Rejects the listing after moderation.
    /// / Recusa o anúncio após a moderação.
    /// </summary>
    public void Reject(
        string rejectionReason,
        DateTime reviewedAtUtc)
    {
        if (Status != ListingStatus.PendingReview)
        {
            throw new InvalidOperationException("Only pending review listings can be rejected.");
        }

        if (string.IsNullOrWhiteSpace(rejectionReason))
        {
            throw new ArgumentException("Rejection reason cannot be empty.");
        }

        Status = ListingStatus.Rejected;
        ReviewedAtUtc = reviewedAtUtc;
        RejectionReason = rejectionReason.Trim();
        UpdatedAtUtc = reviewedAtUtc;
    }
}