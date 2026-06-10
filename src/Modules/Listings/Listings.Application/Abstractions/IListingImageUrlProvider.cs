namespace Listings.Application.Abstractions;

/// <summary>
/// Generates temporary access URLs for listing images.
/// / Gera URLs temporárias de acesso para imagens de anúncios.
/// </summary>
public interface IListingImageUrlProvider
{
    ListingImageAccessUrl Generate(string storageKey);
}

/// <summary>
/// Represents a temporary listing image access URL.
/// / Representa uma URL temporária de acesso à imagem de um anúncio.
/// </summary>
public sealed record ListingImageAccessUrl(
    string Url,
    DateTime ExpiresAtUtc
);