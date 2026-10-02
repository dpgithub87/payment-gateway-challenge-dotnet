using PaymentGateway.Domain.Models;
using PaymentGateway.Infrastructure.Bank.Models;

namespace PaymentGateway.Infrastructure.Bank.Mappers;

public static class BankPaymentMapper
{
    public static BankPaymentRequest ToBankPaymentRequest(this CreatePaymentRequest request)
    {
        return new BankPaymentRequest(
            request.CardNumber,
            $"{request.ExpiryMonth:D2}/{request.ExpiryYear}",
            request.Currency,
            request.Amount,
            request.Cvv);
    }

    public static BankAuthorizationResult.Success ToDomain(this BankPaymentResponse response)
    {
        return new BankAuthorizationResult.Success(
            response.Authorized,
            string.IsNullOrWhiteSpace(response.AuthorizationCode) ? null : response.AuthorizationCode);
    }
}
