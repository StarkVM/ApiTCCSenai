namespace UserAccess.Domain.Helpers;

public static class BirthDate
{
    public static bool IsAdult(this DateTime birthDate, DateTime nowUtc)
    {
        if (birthDate == default)
        {
            return false;
        }
        return (birthDate <= nowUtc.AddYears(-18));
    }
}