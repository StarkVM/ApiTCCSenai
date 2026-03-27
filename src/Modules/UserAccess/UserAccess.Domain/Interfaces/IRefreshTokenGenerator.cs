using UserAccess.Domain.Entities;

namespace UserAccess.Domain.Interfaces;

public interface IRefreshTokenGenerator
{
    string Generate();
}