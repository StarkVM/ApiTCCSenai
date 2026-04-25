namespace UserAccess.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when sending an email fails.
/// / Exceção lançada quando o envio de email falha.
/// </summary>
public sealed class EmailSendFailedException : Exception
{
    public EmailSendFailedException(Exception? innerException = null)
        : base("Failed to send email.", innerException)
    {
    }
}