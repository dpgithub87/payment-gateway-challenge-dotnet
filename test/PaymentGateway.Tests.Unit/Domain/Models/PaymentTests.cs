using AutoFixture;

using PaymentGateway.Domain.Enums;
using PaymentGateway.Domain.Models;
using PaymentGateway.Tests.Unit.TestData;

namespace PaymentGateway.Tests.Unit.Domain.Models;

public class PaymentTests
{
    private readonly Fixture _fixture = new();
    private readonly CreatePaymentRequest _request;
    private readonly string _authorizationCode;
    private readonly DateTimeOffset _now;

    public PaymentTests()
    {
        _request = _fixture.CreatePaymentRequest();
        _authorizationCode = _fixture.Create<Guid>().ToString();
        _now = _fixture.Create<DateTimeOffset>();
    }

    [Fact]
    public void Create_BankAuthorized_ReturnsAuthorizedPayment()
    {
        // Arrange
        var bankResult = new BankAuthorizationResult.Success(true, _authorizationCode);

        // Act
        var payment = Payment.Create(_request, bankResult, _now);

        // Assert
        Assert.Equal(PaymentStatus.Authorized, payment.Status);
        Assert.Equal(_authorizationCode, payment.AuthorizationCode);
    }

    [Fact]
    public void Create_BankDeclined_ReturnsDeclinedPayment()
    {
        // Arrange
        var bankResult = new BankAuthorizationResult.Success(false, null);

        // Act
        var payment = Payment.Create(_request, bankResult, _now);

        // Assert
        Assert.Equal(PaymentStatus.Declined, payment.Status);
        Assert.Null(payment.AuthorizationCode);
    }

    [Fact]
    public void Create_MapsRequestFields()
    {
        // Arrange
        var bankResult = new BankAuthorizationResult.Success(true, _authorizationCode);

        // Act
        var payment = Payment.Create(_request, bankResult, _now);

        // Assert
        Assert.NotEqual(Guid.Empty, payment.Id);
        Assert.Equal("8877", payment.CardNumberLastFour);
        Assert.Equal(_request.ExpiryMonth, payment.ExpiryMonth);
        Assert.Equal(_request.ExpiryYear, payment.ExpiryYear);
        Assert.Equal(_request.Currency, payment.Currency);
        Assert.Equal(_request.Amount, payment.Amount);
        Assert.Equal(_now, payment.CreatedAt);
    }

    [Fact]
    public void Create_GeneratesUniqueIds()
    {
        // Arrange
        var bankResult = new BankAuthorizationResult.Success(true, _authorizationCode);

        // Act
        var first = Payment.Create(_request, bankResult, _now);
        var second = Payment.Create(_request, bankResult, _now);

        // Assert
        Assert.NotEqual(first.Id, second.Id);
    }
}
