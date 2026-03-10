using Serilog.Context;

namespace Api.Infrastructure.Logging;

public sealed class RequestIdMiddleware
{
    private const string HeaderName = "X-Request-Id";
    private readonly RequestDelegate _next;
    
    public RequestIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Português:
        // Tenta reaproveitar um RequestId enviado pelo cliente.
        // English:
        // Tries to reuse a RequestId sent by the client.
        var requestId = context.Request.Headers[HeaderName].FirstOrDefault();
        
        // Português:
        // Se o cliente não enviou nada, geramos um novo identificador.
        // English:
        // If the client did not send one, we generate a new identifier.
        if (string.IsNullOrEmpty(requestId))
        {
            requestId = Guid.NewGuid().ToString("N");
        }
        // Português:
        // Define o identificador interno da request no ASP.NET Core.
        // English:
        // Sets the internal request identifier in ASP.NET Core.
        context.TraceIdentifier = requestId;
        
        // Português:
        // Devolve o mesmo ID no header da resposta para facilitar rastreamento.
        // English:
        // Returns the same ID in the response header to make tracing easier.
        context.Response.Headers[HeaderName] = requestId;
        
        // Português:
        // Empurra o RequestId para o contexto do Serilog durante toda a request.
        // English:
        // Pushes the RequestId into the Serilog context for the whole request.
        using (LogContext.PushProperty("RequestId", requestId))
        {
            await _next(context);
        }
    }
}