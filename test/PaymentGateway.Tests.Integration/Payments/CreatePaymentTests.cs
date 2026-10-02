using System.Net;
using System.Net.Http.Json;
using System.Text;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Tests.Integration.Fixtures;
using PaymentGateway.Tests.Integration.TestData;

namespace PaymentGateway.Tests.Integration.Payments;

public class CreatePaymentTests : IClassFixture<PaymentGatewayApiFactory>
{
    private const string PaymentsUrl = "/api/v1/payments";
    private const string CorrelationIdHeader = "X-Correlation-ID";

    private readonly PaymentGatewayApiFactory _factory;
    private readonly HttpClient _client;

    public CreatePaymentTests(PaymentGatewayApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData(PaymentRequests.AuthorizedCardNumber, PaymentStatus.Authorized)]
    [InlineData(PaymentRequests.DeclinedCardNumber, PaymentStatus.Declined)]
    public async Task CreatePayment_BankResponds_ReturnsCreatedPayment(string cardNumber, PaymentStatus expectedStatus)
    {
        // Arrange
        var request = PaymentRequests.Valid(cardNumber);
        var correlationId = Guid.NewGuid().ToString();
        using var message = new HttpRequestMessage(HttpMethod.Post, PaymentsUrl) { Content = JsonContent.Create(request) };
        message.Headers.Add(CorrelationIdHeader, correlationId);

        // Act
        var response = await _client.SendAsync(message);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var payment = await response.Content.ReadFromJsonAsync<PaymentResponse>(TestJson.Options);
        Assert.NotNull(payment);
        Assert.NotEqual(Guid.Empty, payment.Id);
        Assert.Equal(expectedStatus, payment.Status);
        Assert.Equal(cardNumber[^4..], payment.CardNumberLastFour);
        Assert.Equal(request.ExpiryMonth, payment.ExpiryMonth);
        Assert.Equal(request.ExpiryYear, payment.ExpiryYear);
        Assert.Equal(request.Currency, payment.Currency);
        Assert.Equal(request.Amount, payment.Amount);
        Assert.EndsWith($"{PaymentsUrl}/{payment.Id}", response.Headers.Location!.ToString());
        Assert.Equal(correlationId, response.Headers.GetValues(CorrelationIdHeader).Single());
    }

    [Fact]
    public async Task CreatePayment_BankReturnsServiceUnavailable_ReturnsBadGateway()
    {
        // Arrange
        var request = PaymentRequests.Valid(PaymentRequests.BankUnavailableCardNumber);

        // Act
        var response = await _client.PostAsJsonAsync(PaymentsUrl, request);

        // Assert
        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
    }

    [Fact]
    public async Task CreatePayment_BankUnreachable_ReturnsBadGateway()
    {
        // Arrange
        var client = _factory
            .WithWebHostBuilder(builder => builder.UseSetting("AcquiringBank:BaseUrl", "http://localhost:1/"))
            .CreateClient();

        // Act
        var response = await client.PostAsJsonAsync(PaymentsUrl, PaymentRequests.Valid());

        // Assert
        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
    }

    [Fact]
    public async Task CreatePayment_InvalidRequest_ReturnsValidationProblem()
    {
        // Arrange
        var request = PaymentRequests.Valid() with { CardNumber = "123", Cvv = null };

        // Act
        var response = await _client.PostAsJsonAsync(PaymentsUrl, request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestJson.Options);
        Assert.NotNull(problem);
        Assert.Equal(["cardNumber", "cvv"], problem.Errors.Keys.Order());
    }

    [Fact]
    public async Task CreatePayment_MalformedJson_ReturnsBadRequest()
    {
        // Arrange
        var content = new StringContent("{\"cardNumber\": 123", Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync(PaymentsUrl, content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
