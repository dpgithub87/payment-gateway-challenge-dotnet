using PaymentGateway.Domain.Models;

namespace PaymentGateway.Domain.Ports.Driven;

public interface IAcquiringBankClient
{
    Task<BankAuthorizationResult> AuthorizeAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default);
}
