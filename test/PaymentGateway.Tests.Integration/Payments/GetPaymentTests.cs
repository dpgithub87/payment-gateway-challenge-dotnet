using System.Net;
using System.Net.Http.Json;

using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Tests.Integration.Fixtures;
using PaymentGateway.Tests.Integration.TestData;

namespace PaymentGateway.Tests.Integration.Payments;

public class GetPaymentTests : IClassFixture<PaymentGatewayApiFactory>
{
    private const string PaymentsUrl = "/api/v1/payments";

    private readonly HttpClient _client;

    public GetPaymentTests(PaymentGatewayApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetPayment_ExistingPayment_ReturnsPayment()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync(PaymentsUrl, PaymentRequests.Valid());
        var created = await createResponse.Content.ReadFromJsonAsync<PaymentResponse>(TestJson.Options);

        // Act
        var response = await _client.GetAsync($"{PaymentsUrl}/{created!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payment = await response.Content.ReadFromJsonAsync<PaymentResponse>(TestJson.Options);
        Assert.Equal(created, payment);
    }

    [Fact]
    public async Task GetPayment_UnknownId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync($"{PaymentsUrl}/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPayment_NonGuidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync($"{PaymentsUrl}/not-a-guid");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
