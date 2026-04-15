namespace UserAccess.Infrastructure.Auth.Options;

public sealed class RefreshTokenOptions
{
    public const string SectionName = "Refresh";
    
    public int RefreshTokenDays { get; set; }
}