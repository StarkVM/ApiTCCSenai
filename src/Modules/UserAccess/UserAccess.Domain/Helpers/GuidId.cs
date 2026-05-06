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
        if (!Guid.TryParse(id.ToString(), out Guid guidId))
        {
            return false;
        }
        return true;
    }
}