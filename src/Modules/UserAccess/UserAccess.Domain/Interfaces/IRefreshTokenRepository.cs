using UserAccess.Domain.Entities;

namespace UserAccess.Domain.Interfaces;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token, CancellationToken cancellationToken);
    
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken);
    
     Task<List<RefreshToken>> GetActiveByUseridAsync(Guid userId ,CancellationToken cancellationToken);
}