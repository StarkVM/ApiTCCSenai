namespace UserAccess.Infrastructure.IdentityVerification.Didit.Options;

public class DiditOptions
{
    /// <summary>
    /// Base URL of Didit API.
    /// / URL base da API da Didit.
    /// </summary>
    public string BaseUrl { get; init; } = default!;
    
    /// <summary>
    /// API Key used to authenticate requests.
    /// / API Key usada para autenticar requisições.
    /// </summary>
    public string ApiKey { get; set; } = default!;

    /// <summary>
    /// Secret key used to validate webhook signatures.
    /// / Chave secreta usada para validar assinaturas do webhook.
    /// </summary>
    public string WebhookSecret { get; set; } = default!;

    /// <summary>
    /// Workflow identifier in Didit.
    /// / Identificador do workflow na Didit.
    /// </summary>
    public string WorkflowId { get; set; } = default!;
}