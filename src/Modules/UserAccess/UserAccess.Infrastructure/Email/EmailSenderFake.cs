using UserAccess.Domain.Interfaces;

namespace UserAccess.Infrastructure.Email;

public sealed class EmailSenderFake : IEmailSender
{
    public async Task SendAsync(string email, string subject, string body, CancellationToken cancellationToken)
    {
        email = email.Trim();
        body = body.Trim();
        body = body.Replace("<br/>", Environment.NewLine);

       var fake = await SenderFake(cancellationToken);
    }
    
    public Task<string> SenderFake( CancellationToken cancellationToken)
    {
        return Task.FromResult(string.Empty);
    }
}