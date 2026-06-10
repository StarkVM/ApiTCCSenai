namespace Listings.Application.GetListings.ReadModels;

/// <summary>
/// Represents a listing image read model.
/// / Representa o modelo de leitura de uma imagem do anúncio.
/// </summary>
public sealed record ListingImageReadModel(
    Guid ImageId,
    string StorageKey,
    int DisplayOrder
);