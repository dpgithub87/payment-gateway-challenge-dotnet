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
    public async Task HandleAsync_StoresPendingPaymentBeforeCallingBank()
    {
        // Arrange
        Payment? storedBeforeBankCall = null;
        var storedWhenBankCalled = false;
        _paymentsRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .Callback<Payment, CancellationToken>((payment, _) => storedBeforeBankCall = payment);
        _acquiringBankClientMock
            .Setup(b => b.AuthorizeAsync(It.IsAny<CreatePaymentRequest>(), It.IsAny<CancellationToken>()))
            .Callback(() => storedWhenBankCalled = storedBeforeBankCall is not null)
            .ReturnsAsync(new BankAuthorizationResult.Success(true, _authorizationCode));

        // Act
        await _sut.HandleAsync(_request);

        // Assert
        Assert.True(storedWhenBankCalled);
        Assert.Equal(PaymentStatus.Pending, storedBeforeBankCall!.Status);
        Assert.Equal(_now, storedBeforeBankCall.CreatedAt);
    }

    [Theory]
    [InlineData(true, PaymentStatus.Authorized)]
    [InlineData(false, PaymentStatus.Declined)]
    public async Task HandleAsync_BankResponds_ReturnsAndStoresCompletedPayment(bool authorized, PaymentStatus expectedStatus)
    {
        // Arrange
        SetupBankResponse(new BankAuthorizationResult.Success(authorized, _authorizationCode));

        // Act
        var result = await _sut.HandleAsync(_request);

        // Assert
        Assert.True(result.IsSuccess);
        var success = result.AsSuccess;
        Assert.Equal(expectedStatus, success.Payment.Status);
        _paymentsRepositoryMock.Verify(r => r.UpdateAsync(success.Payment, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_BankUnavailable_ReturnsBankUnavailableErrorAndStoresFailedPayment()
    {
        // Arrange
        SetupBankResponse(new BankAuthorizationResult.BankUnavailableError());

        // Act
        var result = await _sut.HandleAsync(_request);

        // Assert
        Assert.True(result.IsBankUnavailableError);
        _paymentsRepositoryMock.Verify(
            r => r.UpdateAsync(It.Is<Payment>(p => p.Status == PaymentStatus.Failed), CancellationToken.None),
            Times.Once);
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
    public async Task HandleAsync_RepositoryThrows_ReturnsInternalErrorAndDoesNotCallBank()
    {
        // Arrange
        _paymentsRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException());

        // Act
        var result = await _sut.HandleAsync(_request);

        // Assert
        Assert.True(result.IsInternalError);
        _acquiringBankClientMock.Verify(
            b => b.AuthorizeAsync(It.IsAny<CreatePaymentRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
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
