using UserAccess.Domain.Interfaces;
using Resend;

namespace UserAccess.Infrastructure.Email;

public sealed class EmailSender : IEmailSender
{
    private readonly IResend _resend;
    private readonly string _fromEmail;
    
    public EmailSender(IResend resend, string fromEmail)
    {
        _resend = resend;
        _fromEmail = fromEmail;
    }
    public async Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken)
    {
        if (_resend is null)
        {
            throw new NullReferenceException(typeof(EmailSender).Name);
        }
        if (string.IsNullOrWhiteSpace(_fromEmail))
        {
            throw new InvalidOperationException("Sender email is not configured.");
        }
        if (string.IsNullOrWhiteSpace(toEmail))
        {
            throw new ArgumentException("Destination email cannot be null or empty.");
        }

        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new ArgumentException("Subject cannot be null or empty.");
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            throw new ArgumentException("Body cannot be null or empty.");
        }

        var message = new EmailMessage
        {
            From = _fromEmail,
            Subject = subject,
            HtmlBody = body
        };

        message.To.Add(toEmail);
        
        var response = await _resend.EmailSendAsync(message, cancellationToken);

        if (!response.Success)
        {
            throw new InvalidOperationException($"Failed to send email: {response}");
        }
    }
}