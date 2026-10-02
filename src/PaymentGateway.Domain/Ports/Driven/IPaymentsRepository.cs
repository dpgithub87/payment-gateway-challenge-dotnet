using PaymentGateway.Domain.Models;

namespace PaymentGateway.Domain.Ports.Driven;

public interface IPaymentsRepository
{
    Task AddAsync(Payment payment, CancellationToken cancellationToken = default);

    Task<Payment?> GetAsync(Guid id, CancellationToken cancellationToken = default);
}
