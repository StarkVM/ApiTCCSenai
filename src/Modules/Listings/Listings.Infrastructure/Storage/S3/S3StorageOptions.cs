namespace Listings.Infrastructure.Storage.S3;

/// <summary>
/// Configuration options for listing image storage in Amazon S3.
/// / Opções de configuração para armazenamento de imagens de anúncios no Amazon S3.
/// </summary>
public sealed class S3StorageOptions
{
    public const string SectionName = "Listings:S3";

    /// <summary>
    /// S3 bucket name.
    /// / Nome do bucket S3.
    /// </summary>
    public string BucketName { get; init; } = default!;

    /// <summary>
    /// Base folder/prefix used for listing images.
    /// / Pasta/prefixo base usado para imagens de anúncios.
    /// </summary>
    public string BasePrefix { get; init; } = "listings";
    
    public int ReadUrlExpirationMinutes { get; init; } = 15;
}