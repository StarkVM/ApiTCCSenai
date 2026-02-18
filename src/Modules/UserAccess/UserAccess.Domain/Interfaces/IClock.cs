namespace UserAccess.Domain.Interfaces;

public interface IClock
{
    DateTime UtcNow { get; }
}