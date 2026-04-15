namespace UserAccess.Domain.Interfaces;

public interface IRefreshTokenLifetimeProvider
{
    DateTime GetExpirationDateUtc(DateTime nowUtc);
}