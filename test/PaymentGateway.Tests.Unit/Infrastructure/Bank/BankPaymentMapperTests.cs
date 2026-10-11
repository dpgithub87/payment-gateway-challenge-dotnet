using AutoFixture;

using PaymentGateway.Infrastructure.Bank.Mappers;
using PaymentGateway.Infrastructure.Bank.Models;
using PaymentGateway.Tests.Unit.TestData;

namespace PaymentGateway.Tests.Unit.Infrastructure.Bank;

public class BankPaymentMapperTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void ToBankPaymentRequest_MapsFieldsAndFormatsExpiryDate()
    {
        // Arrange
        var request = _fixture.CreatePaymentRequest();

        // Act
        var bankRequest = request.ToBankPaymentRequest();

        // Assert
        Assert.Equal(request.CardNumber, bankRequest.CardNumber);
        Assert.Equal("04/2027", bankRequest.ExpiryDate);
        Assert.Equal(request.Currency, bankRequest.Currency);
        Assert.Equal(request.Amount, bankRequest.Amount);
        Assert.Equal(request.Cvv, bankRequest.Cvv);
    }

    [Fact]
    public void ToDomain_Authorized_MapsAuthorizationCode()
    {
        // Arrange
        var authorizationCode = _fixture.Create<Guid>().ToString();
        var response = new BankPaymentResponse(true, authorizationCode);

        // Act
        var result = response.ToDomain();

        // Assert
        Assert.True(result.Authorized);
        Assert.Equal(authorizationCode, result.AuthorizationCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ToDomain_BlankAuthorizationCode_MapsToNull(string? authorizationCode)
    {
        // Arrange
        var response = new BankPaymentResponse(false, authorizationCode);

        // Act
        var result = response.ToDomain();

        // Assert
        Assert.False(result.Authorized);
        Assert.Null(result.AuthorizationCode);
    }
}
