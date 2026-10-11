using Microsoft.AspNetCore.Http;

namespace PaymentGateway.Api.Errors;

public static class ProblemDetailsCustomization
{
    private const string FrameworkTraceIdKey = "traceId";
    private const string TraceIdKey = "trace_id";

    public static void UseSnakeCaseTraceId(ProblemDetailsContext context)
    {
        if (context.ProblemDetails.Extensions.Remove(FrameworkTraceIdKey, out var traceId))
        {
            context.ProblemDetails.Extensions[TraceIdKey] = traceId;
        }
    }
}
