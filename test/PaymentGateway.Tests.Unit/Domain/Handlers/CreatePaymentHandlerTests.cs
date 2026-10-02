using AutoFixture;

using Microsoft.Extensions.Logging;

using Moq;

using PaymentGateway.Domain.Enums;
using PaymentGateway.Domain.Handlers;
using PaymentGateway.Domain.Models;
using PaymentGateway.Domain.Ports.Driven;
using PaymentGateway.Tests.Unit.TestData;

namespace PaymentGateway.Tests.Unit.Domain.Handlers;

public class CreatePaymentHandlerTests
{
    private readonly Fixture _fixture = new();
    private readonly CreatePaymentRequest _request;
    private readonly string _authorizationCode;
    private readonly DateTimeOffset _now;
    private readonly Mock<IAcquiringBankClient> _acquiringBankClientMock = new();
    private readonly Mock<IPaymentsRepository> _paymentsRepositoryMock = new();
    private readonly Mock<ILogger<CreatePaymentHandler>> _loggerMock = new();
    private readonly CreatePaymentHandler _sut;

    public CreatePaymentHandlerTests()
    {
        _request = _fixture.CreatePaymentRequest();
        _authorizationCode = _fixture.Create<Guid>().ToString();
        _now = _fixture.Create<DateTimeOffset>();

        var timeProviderMock = new Mock<TimeProvider>();
        timeProviderMock.Setup(t => t.GetUtcNow()).Returns(_now);

        _sut = new CreatePaymentHandler(
            _acquiringBankClientMock.Object,
            _paymentsRepositoryMock.Object,
            timeProviderMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_BankAuthorizes_ReturnsAuthorizedPaymentAndStoresIt()
    {
        // Arrange
        SetupBankResponse(new BankAuthorizationResult.Success(true, _authorizationCode));

        // Act
        var result = await _sut.HandleAsync(_request);

        // Assert
        Assert.True(result.IsSuccess);
        var success = result.AsSuccess;
        Assert.Equal(PaymentStatus.Authorized, success.Payment.Status);
        Assert.Equal(_authorizationCode, success.Payment.AuthorizationCode);
        Assert.Equal(_now, success.Payment.CreatedAt);
        _paymentsRepositoryMock.Verify(r => r.AddAsync(success.Payment, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_BankDeclines_ReturnsDeclinedPaymentAndStoresIt()
    {
        // Arrange
        SetupBankResponse(new BankAuthorizationResult.Success(false, null));

        // Act
        var result = await _sut.HandleAsync(_request);

        // Assert
        Assert.True(result.IsSuccess);
        var success = result.AsSuccess;
        Assert.Equal(PaymentStatus.Declined, success.Payment.Status);
        _paymentsRepositoryMock.Verify(r => r.AddAsync(success.Payment, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_BankUnavailable_ReturnsBankUnavailableErrorAndStoresNothing()
    {
        // Arrange
        SetupBankResponse(new BankAuthorizationResult.BankUnavailableError());

        // Act
        var result = await _sut.HandleAsync(_request);

        // Assert
        Assert.True(result.IsBankUnavailableError);
        _paymentsRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_PassesCancellationTokenToBank()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        SetupBankResponse(new BankAuthorizationResult.Success(true, _authorizationCode));

        // Act
        await _sut.HandleAsync(_request, cts.Token);

        // Assert
        _acquiringBankClientMock.Verify(b => b.AuthorizeAsync(_request, cts.Token), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_RepositoryThrows_ReturnsInternalError()
    {
        // Arrange
        SetupBankResponse(new BankAuthorizationResult.Success(true, _authorizationCode));
        _paymentsRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException());

        // Act
        var result = await _sut.HandleAsync(_request);

        // Assert
        Assert.True(result.IsInternalError);
    }

    [Fact]
    public async Task HandleAsync_Cancelled_Throws()
    {
        // Arrange
        _acquiringBankClientMock
            .Setup(b => b.AuthorizeAsync(It.IsAny<CreatePaymentRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        var act = () => _sut.HandleAsync(_request);

        // Assert
        await Assert.ThrowsAsync<OperationCanceledException>(act);
    }

    private void SetupBankResponse(BankAuthorizationResult result)
    {
        _acquiringBankClientMock
            .Setup(b => b.AuthorizeAsync(It.IsAny<CreatePaymentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
    }
}
