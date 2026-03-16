namespace UserAccess.Domain.Interfaces;

public interface IUniIUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken cancellationToken);
}