using System.Text.Json;

using FluentValidation.Results;

using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace PaymentGateway.Api.Validation;

public static class ValidationResultExtensions
{
    public static void CopyErrorsTo(this ValidationResult result, ModelStateDictionary modelState)
    {
        foreach (var error in result.Errors)
        {
            modelState.AddModelError(JsonNamingPolicy.SnakeCaseLower.ConvertName(error.PropertyName), error.ErrorMessage);
        }
    }
}
