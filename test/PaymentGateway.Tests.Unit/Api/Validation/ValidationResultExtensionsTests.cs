using FluentValidation.Results;

using Microsoft.AspNetCore.Mvc.ModelBinding;

using PaymentGateway.Api.Validation;

namespace PaymentGateway.Tests.Unit.Api.Validation;

public class ValidationResultExtensionsTests
{
    [Fact]
    public void CopyErrorsTo_AddsErrorsUnderSnakeCasePropertyNames()
    {
        // Arrange
        var result = new ValidationResult(
        [
            new ValidationFailure("CardNumber", "Card number is required."),
            new ValidationFailure("ExpiryYear", "Expiry year is required."),
            new ValidationFailure("ExpiryYear", "Expiry date must be in the future.")
        ]);
        var modelState = new ModelStateDictionary();

        // Act
        result.CopyErrorsTo(modelState);

        // Assert
        Assert.Equal(["Card number is required."], modelState["card_number"]!.Errors.Select(e => e.ErrorMessage));
        Assert.Equal(["Expiry year is required.", "Expiry date must be in the future."], modelState["expiry_year"]!.Errors.Select(e => e.ErrorMessage));
    }
}
