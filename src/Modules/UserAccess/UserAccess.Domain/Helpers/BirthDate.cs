namespace UserAccess.Domain.Helpers;

public static class BirthDate
{
    public static bool IsAdult(this DateOnly birthDate, DateOnly today)
    {
        if (birthDate == default)
        {
            return false;
        }
        
        return (birthDate <= today.AddYears(-18));
    }
}