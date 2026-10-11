using Microsoft.EntityFrameworkCore;

using PaymentGateway.Infrastructure.Database.Entities;

namespace PaymentGateway.Infrastructure.Database;

public sealed class PaymentGatewayDbContext : DbContext
{
    public PaymentGatewayDbContext(DbContextOptions<PaymentGatewayDbContext> options)
        : base(options)
    {
    }

    public DbSet<PaymentEntity> Payments => Set<PaymentEntity>();
}
