using PaymentGateway.Domain.Enums;

namespace PaymentGateway.Domain.Models;

public sealed record Payment(
    Guid Id,
    PaymentStatus Status,
    string CardNumberLastFour,
    int ExpiryMonth,
    int ExpiryYear,
    string Currency,
    int Amount,
    string? AuthorizationCode,
    DateTimeOffset CreatedAt)
{
    public static Payment Create(
        CreatePaymentRequest request,
        BankAuthorizationResult.Success bankResult,
        DateTimeOffset createdAt)
    {
        return new Payment(
            Guid.NewGuid(),
            bankResult.Authorized ? PaymentStatus.Authorized : PaymentStatus.Declined,
            request.CardNumber[^4..],
            request.ExpiryMonth,
            request.ExpiryYear,
            request.Currency,
            request.Amount,
            bankResult.AuthorizationCode,
            createdAt);
    }
}
