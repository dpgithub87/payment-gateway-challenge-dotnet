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
    public static Payment Create(CreatePaymentRequest request, DateTimeOffset createdAt)
    {
        return new Payment(
            Guid.NewGuid(),
            PaymentStatus.Pending,
            request.CardNumber[^4..],
            request.ExpiryMonth,
            request.ExpiryYear,
            request.Currency,
            request.Amount,
            null,
            createdAt);
    }

    public Payment Complete(BankAuthorizationResult.Success bankResult)
    {
        return this with
        {
            Status = bankResult.Authorized ? PaymentStatus.Authorized : PaymentStatus.Declined,
            AuthorizationCode = bankResult.AuthorizationCode
        };
    }

    public Payment Fail()
    {
        return this with { Status = PaymentStatus.Failed };
    }
}
