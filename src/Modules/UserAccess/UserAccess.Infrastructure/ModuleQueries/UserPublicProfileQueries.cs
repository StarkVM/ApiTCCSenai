using Microsoft.EntityFrameworkCore;
using UserAccess.Contracts.Users.Interfaces;
using UserAccess.Contracts.Users.Records;
using UserAccess.Infrastructure.Persistence;

namespace UserAccess.Infrastructure.ModuleQueries;

/// <summary>
/// Implements public user profile queries.
/// / Implementa consultas de perfis públicos dos usuários.
/// </summary>
public sealed class UserPublicProfileQueries : IUserPublicProfileQueries
{
    private readonly UserAccessDbContext _userAccessDbContext;

    public UserPublicProfileQueries(
        UserAccessDbContext userAccessDbContext)
    {
        _userAccessDbContext = userAccessDbContext;
    }

    public async Task<IReadOnlyCollection<UserPublicProfileSnapshot>> GetByIdsAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken)
    {
        if (userIds.Count == 0)
        {
            return Array.Empty<UserPublicProfileSnapshot>();
        }

        var distinctUserIds = userIds
            .Where(userId => userId != Guid.Empty)
            .Distinct()
            .ToArray();

        return await _userAccessDbContext.Users
            .AsNoTracking()
            .Where(user => distinctUserIds.Contains(user.Id))
            .Select(user => new UserPublicProfileSnapshot(
                user.Id,
                user.FirstName,
                user.LastName))
            .ToArrayAsync(cancellationToken);
    }
}