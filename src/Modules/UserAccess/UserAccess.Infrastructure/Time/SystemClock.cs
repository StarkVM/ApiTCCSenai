using UserAccess.Domain.Interfaces;

namespace UserAccess.Infrastructure.Time;

public class SystemClock : IClock
{
    public DateTime UtcNow { get; }

    public SystemClock()
    {
        UtcNow = DateTime.UtcNow;
    }
    
}