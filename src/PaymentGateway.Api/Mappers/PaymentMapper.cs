using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Domain.Models;

using DomainPaymentStatus = PaymentGateway.Domain.Enums.PaymentStatus;

namespace PaymentGateway.Api.Mappers;

public static class PaymentMapper
{
    public static CreatePaymentRequest ToDomain(this PostPaymentRequest request)
    {
        return new CreatePaymentRequest(
            request.CardNumber!,
            request.ExpiryMonth!.Value,
            request.ExpiryYear!.Value,
            request.Currency!,
            request.Amount!.Value,
            request.Cvv!);
    }

    public static PaymentResponse ToResponse(this Payment payment)
    {
        return new PaymentResponse(
            payment.Id,
            payment.Status.ToApiStatus(),
            payment.CardNumberLastFour,
            payment.ExpiryMonth,
            payment.ExpiryYear,
            payment.Currency,
            payment.Amount);
    }

    private static PaymentStatus ToApiStatus(this DomainPaymentStatus status)
    {
        return status switch
        {
            DomainPaymentStatus.Authorized => PaymentStatus.Authorized,
            DomainPaymentStatus.Declined => PaymentStatus.Declined,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }
}
