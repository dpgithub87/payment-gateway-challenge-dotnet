namespace PaymentGateway.Infrastructure.Bank.Models;

public sealed record BankPaymentRequest(
    string CardNumber,
    string ExpiryDate,
    string Currency,
    int Amount,
    string Cvv);
