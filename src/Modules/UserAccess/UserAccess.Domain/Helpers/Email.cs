using System.Net.Mail;
using System.Security.Cryptography;

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

    public static string Code()
    {
        var number = RandomNumberGenerator.GetInt32(0, 10000000);
        return number.ToString("D6");
    }
}