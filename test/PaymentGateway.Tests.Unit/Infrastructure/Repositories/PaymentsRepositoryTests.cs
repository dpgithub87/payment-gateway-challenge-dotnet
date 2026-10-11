using AutoFixture;

using Microsoft.EntityFrameworkCore;

using PaymentGateway.Domain.Enums;
using PaymentGateway.Infrastructure.Database;
using PaymentGateway.Infrastructure.Repositories;
using PaymentGateway.Tests.Unit.TestData;

namespace PaymentGateway.Tests.Unit.Infrastructure.Repositories;

public class PaymentsRepositoryTests
{
    private readonly Fixture _fixture = new();
    private readonly PaymentGatewayDbContext _dbContext;
    private readonly PaymentsRepository _sut;

    public PaymentsRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<PaymentGatewayDbContext>()
            .UseInMemoryDatabase(_fixture.Create<Guid>().ToString())
            .Options;

        _dbContext = new PaymentGatewayDbContext(options);
        _sut = new PaymentsRepository(_dbContext);
    }

    [Fact]
    public async Task GetAsync_AfterAdd_ReturnsSamePayment()
    {
        // Arrange
        var payment = _fixture.CreatePayment();
        await _sut.AddAsync(payment);

        // Act
        var result = await _sut.GetAsync(payment.Id);

        // Assert
        Assert.Equal(payment, result);
    }

    [Fact]
    public async Task UpdateAsync_ExistingPayment_StoresUpdatedPayment()
    {
        // Arrange
        var payment = _fixture.CreatePayment() with { Status = PaymentStatus.Pending, AuthorizationCode = null };
        await _sut.AddAsync(payment);
        var updated = payment with { Status = PaymentStatus.Authorized, AuthorizationCode = _fixture.Create<Guid>().ToString() };

        // Act
        await _sut.UpdateAsync(updated);

        // Assert
        Assert.Equal(updated, await _sut.GetAsync(payment.Id));
    }

    [Fact]
    public async Task GetAsync_PaymentMissing_ReturnsNull()
    {
        // Act
        var result = await _sut.GetAsync(_fixture.Create<Guid>());

        // Assert
        Assert.Null(result);
    }
}
