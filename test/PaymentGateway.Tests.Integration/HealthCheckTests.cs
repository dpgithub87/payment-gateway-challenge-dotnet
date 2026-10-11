using System.Net;

using PaymentGateway.Tests.Integration.Fixtures;

namespace PaymentGateway.Tests.Integration;

public class HealthCheckTests : IClassFixture<PaymentGatewayApiFactory>
{
    private readonly HttpClient _client;

    public HealthCheckTests(PaymentGatewayApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
