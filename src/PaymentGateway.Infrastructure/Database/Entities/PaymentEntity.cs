using PaymentGateway.Domain.Enums;

namespace PaymentGateway.Infrastructure.Database.Entities;

public sealed class PaymentEntity
{
    public Guid Id { get; set; }

    public PaymentStatus Status { get; set; }

    public string CardNumberLastFour { get; set; } = null!;

    public int ExpiryMonth { get; set; }

    public int ExpiryYear { get; set; }

    public string Currency { get; set; } = null!;

    public int Amount { get; set; }

    public string? AuthorizationCode { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
