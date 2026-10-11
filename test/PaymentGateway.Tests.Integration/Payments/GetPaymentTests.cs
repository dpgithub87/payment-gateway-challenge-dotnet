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
        var createResponse = await _client.PostAsJsonAsync(PaymentsUrl, PaymentRequests.Valid(), TestJson.Options);
        var created = await createResponse.Content.ReadFromJsonAsync<PaymentResponse>(TestJson.Options);

        // Act
        var response = await _client.GetAsync($"{PaymentsUrl}/{created!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payment = await response.Content.ReadFromJsonAsync<PaymentResponse>(TestJson.Options);
        Assert.Equal(created, payment);
    }

    [Theory]
    [InlineData("3f2a9c1e-5b7d-4c8e-9a1f-2d3e4f5a6b7c")]
    [InlineData("not-a-guid")]
    public async Task GetPayment_UnknownOrInvalidId_ReturnsNotFound(string id)
    {
        // Act
        var response = await _client.GetAsync($"{PaymentsUrl}/{id}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
