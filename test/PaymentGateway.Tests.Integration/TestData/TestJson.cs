using System.Text.Json;
using System.Text.Json.Serialization;

namespace PaymentGateway.Tests.Integration.TestData;

public static class TestJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        Converters = { new JsonStringEnumConverter() }
    };
}
