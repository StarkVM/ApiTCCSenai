
namespace UserAccess.Domain.Exceptions.UserAccessExceptions;

/// <summary>
/// Exception thrown when the webhook signature is invalid.
/// / Exceção lançada quando a assinatura do webhook eh invalida.
/// </summary>
public sealed class WebhookInvalidSignatureException: AppException
{
    public WebhookInvalidSignatureException()
        : base(
            code: "WEBHOOK_INVALID_SIGNATURE",
            message: "Webhook invalid signature.")
    {
    }
}


