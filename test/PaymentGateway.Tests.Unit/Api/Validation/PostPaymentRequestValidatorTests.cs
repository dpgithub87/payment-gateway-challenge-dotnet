using AutoFixture;

using FluentValidation.TestHelper;

using Moq;

using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Validation;
using PaymentGateway.Tests.Unit.TestData;

namespace PaymentGateway.Tests.Unit.Api.Validation;

public class PostPaymentRequestValidatorTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 15, 12, 0, 0, TimeSpan.Zero);

    private readonly Fixture _fixture = new();
    private readonly PostPaymentRequest _validRequest;
    private readonly PostPaymentRequestValidator _sut;

    public PostPaymentRequestValidatorTests()
    {
        _validRequest = _fixture.CreatePostPaymentRequest();

        var timeProviderMock = new Mock<TimeProvider>();
        timeProviderMock.Setup(t => t.GetUtcNow()).Returns(Now);

        _sut = new PostPaymentRequestValidator(timeProviderMock.Object);
    }

    [Fact]
    public void Validate_ValidRequest_HasNoErrors()
    {
        // Act
        var result = _sut.TestValidate(_validRequest);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_AllFieldsMissing_HasRequiredErrorForEachField()
    {
        // Arrange
        var request = new PostPaymentRequest(null, null, null, null, null, null);

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r.CardNumber).WithErrorMessage("Card number is required.");
        result.ShouldHaveValidationErrorFor(r => r.ExpiryMonth).WithErrorMessage("Expiry month is required.");
        result.ShouldHaveValidationErrorFor(r => r.ExpiryYear).WithErrorMessage("Expiry year is required.");
        result.ShouldHaveValidationErrorFor(r => r.Currency).WithErrorMessage("Currency is required.");
        result.ShouldHaveValidationErrorFor(r => r.Amount).WithErrorMessage("Amount is required.");
        result.ShouldHaveValidationErrorFor(r => r.Cvv).WithErrorMessage("CVV is required.");
    }

    [Theory]
    [InlineData("2222405343248")]
    [InlineData("22224053432488771234")]
    [InlineData("2222-4053-4324-8877")]
    [InlineData("222240534324887A")]
    public void Validate_InvalidCardNumber_HasCardNumberErrorOnly(string cardNumber)
    {
        // Arrange
        var request = _validRequest with { CardNumber = cardNumber };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r.CardNumber).Only();
    }

    [Theory]
    [InlineData("22224053432488")]
    [InlineData("2222405343248877123")]
    public void Validate_CardNumberAtLengthBoundary_HasNoErrors(string cardNumber)
    {
        // Arrange
        var request = _validRequest with { CardNumber = cardNumber };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public void Validate_InvalidExpiryMonth_HasExpiryMonthErrorOnly(int expiryMonth)
    {
        // Arrange
        var request = _validRequest with { ExpiryMonth = expiryMonth };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r.ExpiryMonth).Only();
    }

    [Theory]
    [InlineData(9, 2026)]
    [InlineData(12, 2025)]
    public void Validate_ExpiryInPast_HasExpiryYearErrorOnly(int expiryMonth, int expiryYear)
    {
        // Arrange
        var request = _validRequest with { ExpiryMonth = expiryMonth, ExpiryYear = expiryYear };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r.ExpiryYear).Only();
    }

    [Fact]
    public void Validate_ExpiryInCurrentMonth_HasNoErrors()
    {
        // Arrange
        var request = _validRequest with { ExpiryMonth = 10, ExpiryYear = 2026 };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("JPY")]
    [InlineData("gbp")]
    [InlineData("GB")]
    [InlineData("GBPX")]
    public void Validate_UnsupportedCurrency_HasCurrencyErrorOnly(string currency)
    {
        // Arrange
        var request = _validRequest with { Currency = currency };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r.Currency).Only();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_NonPositiveAmount_HasAmountErrorOnly(int amount)
    {
        // Arrange
        var request = _validRequest with { Amount = amount };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r.Amount).Only();
    }

    [Theory]
    [InlineData("12")]
    [InlineData("12345")]
    [InlineData("12A")]
    public void Validate_InvalidCvv_HasCvvErrorOnly(string cvv)
    {
        // Arrange
        var request = _validRequest with { Cvv = cvv };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r.Cvv).Only();
    }
}
