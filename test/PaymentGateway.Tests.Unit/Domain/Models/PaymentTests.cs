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
    public void Create_ReturnsPendingPaymentWithRequestFields()
    {
        // Act
        var payment = Payment.Create(_request, _now);

        // Assert
        Assert.NotEqual(Guid.Empty, payment.Id);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
        Assert.Equal("8877", payment.CardNumberLastFour);
        Assert.Equal(_request.ExpiryMonth, payment.ExpiryMonth);
        Assert.Equal(_request.ExpiryYear, payment.ExpiryYear);
        Assert.Equal(_request.Currency, payment.Currency);
        Assert.Equal(_request.Amount, payment.Amount);
        Assert.Null(payment.AuthorizationCode);
        Assert.Equal(_now, payment.CreatedAt);
    }

    [Fact]
    public void Create_GeneratesUniqueIds()
    {
        // Act
        var first = Payment.Create(_request, _now);
        var second = Payment.Create(_request, _now);

        // Assert
        Assert.NotEqual(first.Id, second.Id);
    }

    [Fact]
    public void Complete_BankAuthorized_ReturnsAuthorizedPayment()
    {
        // Arrange
        var payment = Payment.Create(_request, _now);

        // Act
        var completed = payment.Complete(new BankAuthorizationResult.Success(true, _authorizationCode));

        // Assert
        Assert.Equal(PaymentStatus.Authorized, completed.Status);
        Assert.Equal(_authorizationCode, completed.AuthorizationCode);
        Assert.Equal(payment.Id, completed.Id);
    }

    [Fact]
    public void Complete_BankDeclined_ReturnsDeclinedPayment()
    {
        // Arrange
        var payment = Payment.Create(_request, _now);

        // Act
        var completed = payment.Complete(new BankAuthorizationResult.Success(false, null));

        // Assert
        Assert.Equal(PaymentStatus.Declined, completed.Status);
        Assert.Null(completed.AuthorizationCode);
    }

    [Fact]
    public void Fail_ReturnsFailedPayment()
    {
        // Arrange
        var payment = Payment.Create(_request, _now);

        // Act
        var failed = payment.Fail();

        // Assert
        Assert.Equal(PaymentStatus.Failed, failed.Status);
        Assert.Equal(payment.Id, failed.Id);
    }
}
