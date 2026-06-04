using Microsoft.EntityFrameworkCore;
using UserAccess.Contracts.Users.Interfaces;
using UserAccess.Contracts.Users.Records;
using UserAccess.Domain.Enums;
using UserAccess.Infrastructure.Persistence;

namespace UserAccess.Infrastructure.ModuleQueries;

/// <summary>
/// Read-only query implementation exposed by the UserAccess module.
/// / Implementação de consulta somente leitura exposta pelo módulo UserAccess.
/// </summary>
public sealed class UserAccessQueries : IUserAccessQueries
{
    private readonly UserAccessDbContext _userAccessDbContext;

    public UserAccessQueries(UserAccessDbContext userAccessDbContext)
    {
        _userAccessDbContext = userAccessDbContext;
    }

    /// <summary>
    /// Gets the minimal access information of a user for internal module communication.
    /// / Obtém as informações mínimas de acesso de um usuário para comunicação interna entre módulos.
    /// </summary>
    public Task<UserAccessSnapshot?> GetUserAccessSnapshotAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return _userAccessDbContext.Users
            .AsNoTracking()
            .Where(user => user.Id == userId)
            .Select(user => new UserAccessSnapshot(
                user.Id,
                user.Status == UserStatus.Active,
                user.Type == UserType.Provider))
            .FirstOrDefaultAsync(cancellationToken);
    }
}