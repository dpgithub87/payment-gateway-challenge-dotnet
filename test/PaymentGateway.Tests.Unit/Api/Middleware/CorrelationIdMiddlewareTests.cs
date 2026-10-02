using AutoFixture;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using Moq;

using PaymentGateway.Api.Constants;
using PaymentGateway.Api.Middleware;

namespace PaymentGateway.Tests.Unit.Api.Middleware;

public class CorrelationIdMiddlewareTests
{
    private readonly Fixture _fixture = new();
    private readonly Mock<ILogger<CorrelationIdMiddleware>> _loggerMock = new();
    private readonly DefaultHttpContext _httpContext = new();
    private readonly CorrelationIdMiddleware _sut = new(_ => Task.CompletedTask);

    [Theory]
    [InlineData("")]
    [InlineData("contains spaces")]
    [InlineData("new\nline")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    public async Task InvokeAsync_MissingOrInvalidHeader_GeneratesNewId(string incoming)
    {
        // Arrange
        _httpContext.Request.Headers[ApiHeaders.CorrelationId] = incoming;
        _httpContext.TraceIdentifier = _fixture.Create<Guid>().ToString();

        // Act
        await _sut.InvokeAsync(_httpContext, _loggerMock.Object);

        // Assert
        Assert.Equal(_httpContext.TraceIdentifier, _httpContext.Response.Headers[ApiHeaders.CorrelationId]);
    }
}
