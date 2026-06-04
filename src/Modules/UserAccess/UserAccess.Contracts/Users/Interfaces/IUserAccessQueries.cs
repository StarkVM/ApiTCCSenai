using UserAccess.Contracts.Users.Records;

namespace UserAccess.Contracts.Users.Interfaces;

public interface IUserAccessQueries
{
    Task<UserAccessSnapshot?> GetUserAccessSnapshotAsync(
        Guid userId,
        CancellationToken cancellationToken);
}