using Microsoft.EntityFrameworkCore;

using PaymentGateway.Domain.Models;
using PaymentGateway.Domain.Ports.Driven;
using PaymentGateway.Infrastructure.Database;
using PaymentGateway.Infrastructure.Database.Mappers;

namespace PaymentGateway.Infrastructure.Repositories;

public sealed class PaymentsRepository : IPaymentsRepository
{
    private readonly PaymentGatewayDbContext _dbContext;

    public PaymentsRepository(PaymentGatewayDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        await _dbContext.Payments.AddAsync(payment.ToEntity(), cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Payment?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        return entity?.ToDomain();
    }
}
