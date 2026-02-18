using UserAccess.Domain.Entities;

namespace UserAccess.Domain.Interfaces;

public interface IUserRepository
{
    Task<bool> ExistsByEmailAsync(string email);
    Task<bool> ExistsByCpfHashAsync(string cpfHash);
    
    Task AddAsync(User user, Address address);
    
    Task<User?> GetByEmailAsync(string email);
}