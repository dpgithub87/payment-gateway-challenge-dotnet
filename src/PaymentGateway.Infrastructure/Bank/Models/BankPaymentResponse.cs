namespace PaymentGateway.Infrastructure.Bank.Models;

public sealed record BankPaymentResponse(bool Authorized, string? AuthorizationCode);
