using UserAccess.Domain.Entities;

namespace UserAccess.Domain.Interfaces;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token, CancellationToken cancellationToken);
    
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken);
    
     Task<List<RefreshToken>> GetActiveByUserid(Guid userId ,CancellationToken cancellationToken);
}