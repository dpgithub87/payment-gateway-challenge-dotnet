using AutoFixture;

using Microsoft.EntityFrameworkCore;

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
    public async Task GetAsync_PaymentMissing_ReturnsNull()
    {
        // Act
        var result = await _sut.GetAsync(_fixture.Create<Guid>());

        // Assert
        Assert.Null(result);
    }
}
