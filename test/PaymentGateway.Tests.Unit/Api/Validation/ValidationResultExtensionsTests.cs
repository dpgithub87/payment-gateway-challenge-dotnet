using FluentValidation.Results;

using Microsoft.AspNetCore.Mvc.ModelBinding;

using PaymentGateway.Api.Validation;

namespace PaymentGateway.Tests.Unit.Api.Validation;

public class ValidationResultExtensionsTests
{
    [Fact]
    public void AddToModelState_AddsErrorsUnderCamelCasePropertyNames()
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
        result.AddToModelState(modelState);

        // Assert
        Assert.Equal(["Card number is required."], modelState["cardNumber"]!.Errors.Select(e => e.ErrorMessage));
        Assert.Equal(["Expiry year is required.", "Expiry date must be in the future."], modelState["expiryYear"]!.Errors.Select(e => e.ErrorMessage));
    }
}
