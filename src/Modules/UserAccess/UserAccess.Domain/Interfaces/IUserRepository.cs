using UserAccess.Domain.Entities;

namespace UserAccess.Domain.Interfaces;

public interface IUserRepository
{
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);
    Task<bool> CpfHashExistsAsync(string cpfHash, CancellationToken cancellationToken);
    Task AddAsync(User user, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
    Task DisableExpiredAsync(DateTime nowUtc, CancellationToken cancellationToken);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    public Task<User?> GetByCpfAsync(string cpfHash, CancellationToken cancellationToken);
    Task DeleteDisabledByEmailAsync(string email, CancellationToken cancellationToken);
    Task<bool> CpfHashExistsForAnotherUserAsync(string cpfHash, Guid userId, CancellationToken cancellationToken);
}