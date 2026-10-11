using AutoFixture;

using Microsoft.Extensions.Logging;

using Moq;

using PaymentGateway.Domain.Handlers;
using PaymentGateway.Domain.Models;
using PaymentGateway.Domain.Ports.Driven;
using PaymentGateway.Tests.Unit.TestData;

namespace PaymentGateway.Tests.Unit.Domain.Handlers;

public class GetPaymentHandlerTests
{
    private readonly Fixture _fixture = new();
    private readonly Mock<IPaymentsRepository> _paymentsRepositoryMock = new();
    private readonly Mock<ILogger<GetPaymentHandler>> _loggerMock = new();
    private readonly GetPaymentHandler _sut;

    public GetPaymentHandlerTests()
    {
        _sut = new GetPaymentHandler(_paymentsRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_PaymentExists_ReturnsSuccess()
    {
        // Arrange
        var payment = _fixture.CreatePayment();
        _paymentsRepositoryMock
            .Setup(r => r.GetAsync(payment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        // Act
        var result = await _sut.HandleAsync(new GetPaymentRequest(payment.Id));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Same(payment, result.AsSuccess.Payment);
    }

    [Fact]
    public async Task HandleAsync_PaymentMissing_ReturnsNotFound()
    {
        // Arrange
        _paymentsRepositoryMock
            .Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        // Act
        var result = await _sut.HandleAsync(new GetPaymentRequest(_fixture.Create<Guid>()));

        // Assert
        Assert.True(result.IsNotFound);
    }
}
