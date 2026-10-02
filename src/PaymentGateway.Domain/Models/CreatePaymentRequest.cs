namespace PaymentGateway.Domain.Models;

public sealed record CreatePaymentRequest(
    string CardNumber,
    int ExpiryMonth,
    int ExpiryYear,
    string Currency,
    int Amount,
    string Cvv);
