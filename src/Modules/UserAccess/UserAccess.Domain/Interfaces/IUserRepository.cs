using UserAccess.Domain.Entities;

namespace UserAccess.Domain.Interfaces;

public interface IUserRepository
{
    public Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);
    Task<bool> CpfHashExistsAsync(string cpfHash, CancellationToken cancellationToken);
    Task AddAsync(User user, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    public Task<User?> GetByCpfAsync(string cpfHash, CancellationToken cancellationToken);
    Task<bool> CpfHashExistsForAnotherUserAsync(string cpfHash, Guid userId, CancellationToken cancellationToken);
}