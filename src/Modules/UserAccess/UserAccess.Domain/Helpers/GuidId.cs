namespace UserAccess.Domain.Helpers;

public static class GuidId
{
    public static bool GuidIdIsValid(this Guid id)
    {
        if (id == Guid.Empty)
        {
            return false;
        }
        if (string.IsNullOrWhiteSpace(id.ToString()))
        {
            return false;
        }
        return true;
    }
}