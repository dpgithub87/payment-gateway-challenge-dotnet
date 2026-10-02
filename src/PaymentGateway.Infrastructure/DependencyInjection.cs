using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using PaymentGateway.Domain.Ports.Driven;
using PaymentGateway.Infrastructure.Bank;
using PaymentGateway.Infrastructure.Database;
using PaymentGateway.Infrastructure.Repositories;

namespace PaymentGateway.Infrastructure;

public static class DependencyInjection
{
    private const string DatabaseName = "PaymentGateway";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PaymentGatewayDbContext>(options => options.UseInMemoryDatabase(DatabaseName));
        services.AddScoped<IPaymentsRepository, PaymentsRepository>();

        services.Configure<AcquiringBankOptions>(configuration.GetSection(AcquiringBankOptions.SectionName));
        services.AddHttpClient<IAcquiringBankClient, AcquiringBankClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<AcquiringBankOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = options.Timeout;
        });

        return services;
    }
}
