using AutoFixture;

using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Tests.Unit.TestData;

namespace PaymentGateway.Tests.Unit.Api.Models;

public class PostPaymentRequestTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void Constructor_LowercaseCurrency_NormalisesToUppercase()
    {
        // Act
        var request = new PostPaymentRequest("2222405343248877", 4, 2027, "gbp", 100, "123");

        // Assert
        Assert.Equal("GBP", request.Currency);
    }

    [Fact]
    public void With_LowercaseCurrency_NormalisesToUppercase()
    {
        // Act
        var request = _fixture.CreatePostPaymentRequest() with { Currency = "usd" };

        // Assert
        Assert.Equal("USD", request.Currency);
    }
}
