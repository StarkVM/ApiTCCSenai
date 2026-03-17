using UserAccess.Domain.Interfaces;

namespace UserAccess.Infrastructure.Email;

public sealed class EmailSenderFake : IEmailSender
{
    public async Task SendAsync(string email, string subject, string htmlMessage, CancellationToken cancellationToken)
    {
        email = email.Trim();
        htmlMessage = htmlMessage.Trim();
        htmlMessage = htmlMessage.Replace("<br/>", Environment.NewLine);

       var fake = await SenderFake(cancellationToken);
    }
    
    public Task<string> SenderFake( CancellationToken cancellationToken)
    {
        return Task.FromResult(string.Empty);
    }
}