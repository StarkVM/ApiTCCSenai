namespace UserAccess.Domain.Interfaces;

public interface IEmailSender
{
    Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken);
    
    Task<string> SenderFake( CancellationToken cancellationToken);
}