namespace Rentals.Domain.Interfaces;

public interface IClock
{
    DateTime UtcNow { get; }
}