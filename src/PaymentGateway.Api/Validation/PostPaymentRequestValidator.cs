using System.Text.RegularExpressions;

using FluentValidation;

using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Validation;

public sealed partial class PostPaymentRequestValidator : AbstractValidator<PostPaymentRequest>
{
    private static readonly HashSet<string> SupportedCurrencies = ["GBP", "USD", "EUR"];

    private readonly TimeProvider _timeProvider;

    public PostPaymentRequestValidator(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;

        RuleFor(r => r.CardNumber)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Card number is required.")
            .Matches(CardNumberRegex()).WithMessage("Card number must be 14-19 digits.");

        RuleFor(r => r.ExpiryMonth)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Expiry month is required.")
            .InclusiveBetween(1, 12).WithMessage("Expiry month must be between 1 and 12.");

        RuleFor(r => r.ExpiryYear)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Expiry year is required.")
            .Must((request, year) => IsInFuture(request.ExpiryMonth!.Value, year!.Value))
            .When(r => r.ExpiryMonth is >= 1 and <= 12, ApplyConditionTo.CurrentValidator)
            .WithMessage("Expiry date must be in the future.");

        RuleFor(r => r.Currency)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Currency is required.")
            .Must(currency => SupportedCurrencies.Contains(currency!))
            .WithMessage($"Currency must be one of: {string.Join(", ", SupportedCurrencies)}.");

        RuleFor(r => r.Amount)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Amount is required.")
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");

        RuleFor(r => r.Cvv)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("CVV is required.")
            .Matches(CvvRegex()).WithMessage("CVV must be 3-4 digits.");
    }

    private bool IsInFuture(int month, int year)
    {
        var now = _timeProvider.GetUtcNow();

        return year > now.Year || (year == now.Year && month >= now.Month);
    }

    [GeneratedRegex(@"^\d{14,19}$")]
    private static partial Regex CardNumberRegex();

    [GeneratedRegex(@"^\d{3,4}$")]
    private static partial Regex CvvRegex();
}