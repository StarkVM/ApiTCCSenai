namespace UserAccess.Domain.Exceptions.UserAccessExceptions;

/// <summary>
/// Exception thrown when the payload is invalid.
/// / Exceção lançada quando payload eh invalido
/// </summary>
public class WebhookInvalidPayloadException : AppException
{
    public WebhookInvalidPayloadException()
        : base(
            code: "WEBHOOK_INVALID_PAYLOAD",
            message: "Webhook invalid payload.")
    {
    }
}