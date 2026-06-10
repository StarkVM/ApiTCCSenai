using Rentals.Domain.Interfaces;

namespace Rentals.Infrastructure.Time;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}