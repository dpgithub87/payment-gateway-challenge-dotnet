using System.Net;
using System.Net.Http.Json;
using System.Text;

using AutoFixture;
using Microsoft.Extensions.Logging;

using Moq;
using Moq.Protected;

using PaymentGateway.Domain.Models;
using PaymentGateway.Infrastructure.Bank;
using PaymentGateway.Tests.Unit.TestData;

namespace PaymentGateway.Tests.Unit.Infrastructure.Bank;

public class AcquiringBankClientTests
{
    private readonly Fixture _fixture = new();
    private readonly CreatePaymentRequest _request;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new();
    private readonly Mock<ILogger<AcquiringBankClient>> _loggerMock = new();
    private readonly AcquiringBankClient _sut;

    public AcquiringBankClientTests()
    {
        _request = _fixture.CreatePaymentRequest();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:8080/")
        };

        _sut = new AcquiringBankClient(httpClient, _loggerMock.Object);
    }

    [Fact]
    public async Task AuthorizeAsync_BankAuthorizes_ReturnsAuthorizedSuccess()
    {
        // Arrange
        var authorizationCode = _fixture.Create<Guid>().ToString();
        SetupHttpResponse(HttpStatusCode.OK, new { authorized = true, authorization_code = authorizationCode });

        // Act
        var result = await _sut.AuthorizeAsync(_request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.AsSuccess.Authorized);
        Assert.Equal(authorizationCode, result.AsSuccess.AuthorizationCode);
    }

    [Fact]
    public async Task AuthorizeAsync_BankDeclines_ReturnsDeclinedSuccessWithNullCode()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK, new { authorized = false, authorization_code = "" });

        // Act
        var result = await _sut.AuthorizeAsync(_request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.AsSuccess.Authorized);
        Assert.Null(result.AsSuccess.AuthorizationCode);
    }

    [Theory]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task AuthorizeAsync_BankReturnsNonSuccess_ReturnsBankUnavailableError(HttpStatusCode statusCode)
    {
        // Arrange
        SetupHttpResponse(statusCode, new { });

        // Act
        var result = await _sut.AuthorizeAsync(_request);

        // Assert
        Assert.True(result.IsBankUnavailableError);
    }

    [Fact]
    public async Task AuthorizeAsync_RequestFails_ReturnsBankUnavailableError()
    {
        // Arrange
        SetupHttpException(new HttpRequestException());

        // Act
        var result = await _sut.AuthorizeAsync(_request);

        // Assert
        Assert.True(result.IsBankUnavailableError);
    }

    [Fact]
    public async Task AuthorizeAsync_RequestTimesOut_ReturnsBankUnavailableError()
    {
        // Arrange
        SetupHttpException(new TaskCanceledException());

        // Act
        var result = await _sut.AuthorizeAsync(_request);

        // Assert
        Assert.True(result.IsBankUnavailableError);
    }

    [Theory]
    [InlineData("not json", "application/json")]
    [InlineData("<html>error</html>", "text/html")]
    [InlineData("null", "application/json")]
    public async Task AuthorizeAsync_BankReturnsInvalidBody_ReturnsBankUnavailableError(string body, string mediaType)
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK, new StringContent(body, Encoding.UTF8, mediaType));

        // Act
        var result = await _sut.AuthorizeAsync(_request);

        // Assert
        Assert.True(result.IsBankUnavailableError);
    }

    [Fact]
    public async Task AuthorizeAsync_CallerCancels_Throws()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        SetupHttpException(new TaskCanceledException());

        // Act
        var act = () => _sut.AuthorizeAsync(_request, cts.Token);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(act);
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, object body)
    {
        SetupHttpResponse(statusCode, JsonContent.Create(body));
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, HttpContent content)
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(statusCode) { Content = content });
    }

    private void SetupHttpException(Exception exception)
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(exception);
    }
}
