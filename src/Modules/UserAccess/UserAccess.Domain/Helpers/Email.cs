using System.Net.Mail;

namespace UserAccess.Domain.Helpers;

public static class Email
{
    public static bool EmailIsValid(this string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }
        try
        {
            _ = new MailAddress(email);
            return true;
        }
        catch
        {
            return false;
        }
    }
}