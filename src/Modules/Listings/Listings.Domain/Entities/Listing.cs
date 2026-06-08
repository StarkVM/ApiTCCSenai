using Listings.Domain.Enums;
using Listings.Domain.ValueObjects;

namespace Listings.Domain.Entities;

/// <summary>
/// Represents a machinery rental listing created by a provider.
/// / Representa um anúncio de aluguel de máquina criado por um fornecedor.
/// </summary>
public sealed class Listing
{
    private const long MaxImageSizeInBytes = 5 * 1024 * 1024;
    private const int MaxImages = 5;
    private const int MinImages = 1;

    private readonly List<ListingImage> _images = [];

    public Guid Id { get; private set; }
    public Guid OwnerId { get; private set; }
    public string Title { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public ListingCategory Category { get; private set; }
    public decimal DailyPrice { get; private set; }
    public PickupAddress PickupAddress { get; private set; } = default!;
    public OperatorOption OperatorOption { get; private set; } = default!;
    public FreightOption FreightOption { get; private set; } = default!;
    public ListingStatus Status { get; private set; } = ListingStatus.PendingReview;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public DateTime? ReviewedAtUtc { get; private set; }
    public string? RejectionReason { get; private set; }
    public bool IsFleet { get; private set; }
    

public IReadOnlyCollection<ListingImage> Images => _images.AsReadOnly();

    private Listing()
    {
    }

    public Listing(
        Guid id,
        Guid ownerId,
        string title,
        string description,
        ListingCategory category,
        decimal dailyPrice,
        PickupAddress pickupAddress,
        OperatorOption operatorOption,
        FreightOption freightOption,
        DateTime createdAtUtc,
        bool isFleet)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Listing id cannot be empty.");
        }

        if (ownerId == Guid.Empty)
        {
            throw new ArgumentException("Owner id cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title cannot be empty.");
        }

        if (title.Trim().Length > 150)
        {
            throw new ArgumentException("Title cannot have more than 150 characters.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Description cannot be empty.");
        }

        if (description.Trim().Length > 2000)
        {
            throw new ArgumentException("Description cannot have more than 2000 characters.");
        }

        if (category == ListingCategory.Unknown)
        {
            throw new ArgumentException("Listing category is required.");
        }

        if (dailyPrice <= 0)
        {
            throw new ArgumentException("Daily price must be greater than zero.");
        }

        ArgumentNullException.ThrowIfNull(pickupAddress);
        ArgumentNullException.ThrowIfNull(operatorOption);
        ArgumentNullException.ThrowIfNull(freightOption);

        Id = id;
        OwnerId = ownerId;
        Title = title.Trim();
        Description = description.Trim();
        Category = category;
        DailyPrice = dailyPrice;
        PickupAddress = pickupAddress;
        OperatorOption = operatorOption;
        FreightOption = freightOption;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
        Status = ListingStatus.Approved;
        IsFleet = isFleet;
    }

    public static bool ListingImageSizeIsValid(long size)
    {
        if (size >= MaxImageSizeInBytes)
        {
            return false;
        }
        return true;
    }
    
    public void Approve(DateTime reviewedAtUtc)
    {
        if (Status != ListingStatus.PendingReview)
        {
            throw new InvalidOperationException("Only pending review listings can be approved.");
        }
        
        EnsureHasMinimumImages();
        
        Status = ListingStatus.Approved;
        ReviewedAtUtc = reviewedAtUtc;
        RejectionReason = null;
        UpdatedAtUtc = reviewedAtUtc;
    }
    
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
    
    public void Edit(
        string title,
        string description,
        ListingCategory category,
        decimal dailyPrice,
        PickupAddress pickupAddress,
        OperatorOption operatorOption,
        FreightOption freightOption,
        DateTime updatedAtUtc,
        bool isFleet)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title cannot be empty.");
        }

        var normalizedTitle = title.Trim();

        if (normalizedTitle.Length > 150)
        {
            throw new ArgumentException("Title cannot have more than 150 characters.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Description cannot be empty.");
        }

        var normalizedDescription = description.Trim();

        if (normalizedDescription.Length > 2000)
        {
            throw new ArgumentException("Description cannot have more than 2000 characters.");
        }

        if (category == ListingCategory.Unknown)
        {
            throw new ArgumentException("Listing category is required.");
        }

        if (dailyPrice <= 0)
        {
            throw new ArgumentException("Daily price must be greater than zero.");
        }

        ArgumentNullException.ThrowIfNull(pickupAddress);
        ArgumentNullException.ThrowIfNull(operatorOption);
        ArgumentNullException.ThrowIfNull(freightOption);

        Title = normalizedTitle;
        Description = normalizedDescription;
        Category = category;
        DailyPrice = dailyPrice;
        PickupAddress = pickupAddress;
        OperatorOption = operatorOption;
        FreightOption = freightOption;
        UpdatedAtUtc = updatedAtUtc;
        IsFleet = isFleet;

        //SendBackToReviewIfNeeded();
    }
    
    public void AddImage(
        Guid imageId,
        string storageKey,
        int displayOrder,
        DateTime createdAtUtc)
    {
        if (imageId == Guid.Empty)
        {
            throw new ArgumentException("Image id cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(storageKey))
        {
            throw new ArgumentException("Storage key cannot be empty.");
        }

        if (_images.Count >= MaxImages)
        {
            throw new InvalidOperationException("A listing cannot contain more than five images.");
        }

        if (displayOrder < 1 || displayOrder > MaxImages)
        {
            throw new ArgumentException("Display order must be between 1 and 5.");
        }

        if (_images.Any(image => image.Id == imageId))
        {
            throw new InvalidOperationException("This image already exists in the listing.");
        }

        if (_images.Any(image => image.StorageKey == storageKey.Trim()))
        {
            throw new InvalidOperationException("This image storage key is already being used.");
        }

        if (_images.Any(image => image.DisplayOrder == displayOrder))
        {
            throw new InvalidOperationException("This display order is already being used.");
        }

        var image = new ListingImage(
            imageId,
            Id,
            storageKey.Trim(),
            displayOrder,
            createdAtUtc);

        _images.Add(image);
        UpdatedAtUtc = createdAtUtc;

        //SendBackToReviewIfNeeded();
    }
    
    public string RemoveImage(
        Guid imageId,
        DateTime removedAtUtc)
    {
        if (imageId == Guid.Empty)
        {
            throw new ArgumentException("Image id cannot be empty.");
        }
        
        if (_images.Count <= MinImages)
        {
            throw new InvalidOperationException("A listing must contain at least one image.");
        }

        var image = _images.FirstOrDefault(x => x.Id == imageId);

        if (image is null)
        {
            throw new InvalidOperationException("Image was not found in this listing.");
        }

        _images.Remove(image);

        UpdatedAtUtc = removedAtUtc;

        return image.StorageKey;
    }
    
    private void SendBackToReviewIfNeeded()
    {
        if (Status == ListingStatus.PendingReview)
        {
            return;
        }

        if (Status == ListingStatus.Approved ||
            Status == ListingStatus.Rejected)
        {
            Status = ListingStatus.PendingReview;
            ReviewedAtUtc = null;
            RejectionReason = null;
            return;
        }

        throw new InvalidOperationException("This listing cannot be edited in its current status.");
    }

    private void EnsureHasMinimumImages()
    {
        if (_images.Count < MinImages)
        {
            throw new InvalidOperationException("A listing must contain at least one image.");
        }
    }
}