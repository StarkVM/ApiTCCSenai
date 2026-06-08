using Listings.Domain.Interfaces;

namespace Listings.Infrastructure.Time;

public class SystemClock : IClock
{
    public DateTime UtcNow { get; }

    public SystemClock()
    {
        UtcNow = DateTime.UtcNow;
    }

}