namespace UserAccess.Domain.Interfaces;

public interface IAccessTokenLifetimeProvider
{
    DateTime GetExpirationDateUtc(DateTime nowUtc);
}