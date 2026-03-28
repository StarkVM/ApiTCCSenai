namespace UserAccess.Infrastructure.Auth;

public sealed class RefreshTokenOptions
{
    public const string SectionName = "RefreshToken";
    
    public int RefreshTokenDays { get; set; }
}