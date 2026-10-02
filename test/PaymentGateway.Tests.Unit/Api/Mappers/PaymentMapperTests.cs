using AutoFixture;

using PaymentGateway.Api.Mappers;
using PaymentGateway.Api.Models;
using PaymentGateway.Tests.Unit.TestData;

using DomainPaymentStatus = PaymentGateway.Domain.Enums.PaymentStatus;

namespace PaymentGateway.Tests.Unit.Api.Mappers;

public class PaymentMapperTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void ToDomain_MapsAllFields()
    {
        // Arrange
        var request = _fixture.CreatePostPaymentRequest();

        // Act
        var domainRequest = request.ToDomain();

        // Assert
        Assert.Equal(request.CardNumber, domainRequest.CardNumber);
        Assert.Equal(request.ExpiryMonth, domainRequest.ExpiryMonth);
        Assert.Equal(request.ExpiryYear, domainRequest.ExpiryYear);
        Assert.Equal(request.Currency, domainRequest.Currency);
        Assert.Equal(request.Amount, domainRequest.Amount);
        Assert.Equal(request.Cvv, domainRequest.Cvv);
    }

    [Theory]
    [InlineData(DomainPaymentStatus.Authorized, PaymentStatus.Authorized)]
    [InlineData(DomainPaymentStatus.Declined, PaymentStatus.Declined)]
    public void ToResponse_MapsAllFields(DomainPaymentStatus domainStatus, PaymentStatus expectedStatus)
    {
        // Arrange
        var payment = _fixture.CreatePayment() with { Status = domainStatus };

        // Act
        var response = payment.ToResponse();

        // Assert
        Assert.Equal(payment.Id, response.Id);
        Assert.Equal(expectedStatus, response.Status);
        Assert.Equal(payment.CardNumberLastFour, response.CardNumberLastFour);
        Assert.Equal(payment.ExpiryMonth, response.ExpiryMonth);
        Assert.Equal(payment.ExpiryYear, response.ExpiryYear);
        Assert.Equal(payment.Currency, response.Currency);
        Assert.Equal(payment.Amount, response.Amount);
    }
}
