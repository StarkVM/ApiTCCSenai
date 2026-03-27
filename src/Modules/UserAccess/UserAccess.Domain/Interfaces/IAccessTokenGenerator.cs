using UserAccess.Domain.Entities;

namespace UserAccess.Domain.Interfaces;

public interface IAccessTokenGenerator
{
    string Generate(User user);
}