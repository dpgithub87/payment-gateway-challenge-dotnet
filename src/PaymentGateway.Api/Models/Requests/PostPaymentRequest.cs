namespace PaymentGateway.Api.Models.Requests;

public sealed record PostPaymentRequest(
    string? CardNumber,
    int? ExpiryMonth,
    int? ExpiryYear,
    string? Currency,
    int? Amount,
    string? Cvv)
{
    private readonly string? _currency = Currency?.ToUpperInvariant();

    public string? Currency
    {
        get => _currency;
        init => _currency = value?.ToUpperInvariant();
    }
}
