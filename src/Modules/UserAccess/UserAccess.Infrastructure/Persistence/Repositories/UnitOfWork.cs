using UserAccess.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using UserAccess.Domain.Entities;

namespace UserAccess.Infrastructure.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly UserAccessDbContext _userAccessDbContext;

    public UnitOfWork(UserAccessDbContext userAccessDbContext)
    {
        _userAccessDbContext = userAccessDbContext;
    }
    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _userAccessDbContext.SaveChangesAsync(cancellationToken);
    }
}