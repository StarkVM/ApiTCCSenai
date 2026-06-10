using UserAccess.Contracts.Users.Records;

namespace UserAccess.Contracts.Users.Interfaces;

public interface IUserPublicProfileQueries
{
    Task<IReadOnlyCollection<UserPublicProfileSnapshot>> GetByIdsAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken);
}