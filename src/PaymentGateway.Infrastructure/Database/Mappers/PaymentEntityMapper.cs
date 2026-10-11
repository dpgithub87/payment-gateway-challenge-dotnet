using PaymentGateway.Domain.Models;
using PaymentGateway.Infrastructure.Database.Entities;

namespace PaymentGateway.Infrastructure.Database.Mappers;

public static class PaymentEntityMapper
{
    public static PaymentEntity ToEntity(this Payment payment)
    {
        return new PaymentEntity
        {
            Id = payment.Id,
            Status = payment.Status,
            CardNumberLastFour = payment.CardNumberLastFour,
            ExpiryMonth = payment.ExpiryMonth,
            ExpiryYear = payment.ExpiryYear,
            Currency = payment.Currency,
            Amount = payment.Amount,
            AuthorizationCode = payment.AuthorizationCode,
            CreatedAt = payment.CreatedAt
        };
    }

    public static Payment ToDomain(this PaymentEntity entity)
    {
        return new Payment(
            entity.Id,
            entity.Status,
            entity.CardNumberLastFour,
            entity.ExpiryMonth,
            entity.ExpiryYear,
            entity.Currency,
            entity.Amount,
            entity.AuthorizationCode,
            entity.CreatedAt);
    }
}
