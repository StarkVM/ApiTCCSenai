using UserAccess.Domain.Results;

namespace UserAccess.Domain.Interfaces;

public interface IIdentityVerificationWebhookParser
{
    IdentityVerificationWebhookParserResult Parse(string rawBody);
}