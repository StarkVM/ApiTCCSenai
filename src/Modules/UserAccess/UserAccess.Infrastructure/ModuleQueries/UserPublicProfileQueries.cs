using Microsoft.EntityFrameworkCore;
using UserAccess.Application.Abstractions;
using UserAccess.Contracts.Users.Interfaces;
using UserAccess.Contracts.Users.Records;
using UserAccess.Domain.Enums;
using UserAccess.Infrastructure.Persistence;

namespace UserAccess.Infrastructure.ModuleQueries;

/// <summary>
/// Implements public user profile queries.
/// / Implementa consultas de perfis públicos dos usuários.
/// </summary>
public sealed class UserPublicProfileQueries : IUserPublicProfileQueries
{
    private readonly UserAccessDbContext _userAccessDbContext;
    private readonly IUserProfilePhotoUrlProvider _profilePhotoUrlProvider;

    public UserPublicProfileQueries(
        UserAccessDbContext userAccessDbContext,
        IUserProfilePhotoUrlProvider profilePhotoUrlProvider)
    {
        _userAccessDbContext = userAccessDbContext;
        _profilePhotoUrlProvider = profilePhotoUrlProvider;
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
                user.LastName,
                user.Status == UserStatus.Active
                ))
            .ToArrayAsync(cancellationToken);
    }
    
    public async Task<UserPublicProfileWithPhotoSnapshot?> GetByIdWithPhotoAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            return null;
        }

        var user = await _userAccessDbContext.Users
            .AsNoTracking()
            .Where(currentUser => currentUser.Id == userId)
            .Select(currentUser => new
            {
                currentUser.Id,
                currentUser.FirstName,
                currentUser.LastName,
                currentUser.ProfilePhotoStorageKey
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return null;
        }

        string? profilePhotoUrl = null;
        DateTime? profilePhotoUrlExpiresAtUtc = null;

        if (!string.IsNullOrWhiteSpace(user.ProfilePhotoStorageKey))
        {
            var accessUrl = _profilePhotoUrlProvider.Generate(
                user.ProfilePhotoStorageKey);

            profilePhotoUrl = accessUrl.Url;
            profilePhotoUrlExpiresAtUtc = accessUrl.ExpiresAtUtc;
        }

        return new UserPublicProfileWithPhotoSnapshot(
            user.Id,
            user.FirstName,
            user.LastName,
            profilePhotoUrl,
            profilePhotoUrlExpiresAtUtc);
    }
}