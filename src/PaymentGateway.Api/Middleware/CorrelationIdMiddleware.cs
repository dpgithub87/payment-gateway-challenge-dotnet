using System.Diagnostics;
using System.Text.RegularExpressions;

using PaymentGateway.Api.Constants;

namespace PaymentGateway.Api.Middleware;

public sealed partial class CorrelationIdMiddleware
{
    private const string CorrelationIdScope = "CorrelationId:{CorrelationId}";
    private const string CorrelationIdTag = "correlation.id";

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ILogger<CorrelationIdMiddleware> logger)
    {
        var correlationId = GetOrCreateCorrelationId(context);

        context.Response.Headers[ApiHeaders.CorrelationId] = correlationId;
        Activity.Current?.SetTag(CorrelationIdTag, correlationId);

        using (logger.BeginScope(CorrelationIdScope, correlationId))
        {
            await _next(context);
        }
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        var incoming = context.Request.Headers[ApiHeaders.CorrelationId].ToString();

        if (ValidCorrelationIdRegex().IsMatch(incoming))
        {
            return incoming;
        }

        return Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
    }

    [GeneratedRegex("^[A-Za-z0-9-]{1,64}$")]
    private static partial Regex ValidCorrelationIdRegex();
}
