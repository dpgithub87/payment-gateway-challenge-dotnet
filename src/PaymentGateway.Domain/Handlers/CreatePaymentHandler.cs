using Microsoft.Extensions.Logging;

using PaymentGateway.Domain.Models;
using PaymentGateway.Domain.Ports.Driven;
using PaymentGateway.Domain.Ports.Driving;

namespace PaymentGateway.Domain.Handlers;

public sealed class CreatePaymentHandler : IRequestHandler<CreatePaymentRequest, CreatePaymentResult>
{
    private readonly IAcquiringBankClient _bankClient;
    private readonly IPaymentsRepository _paymentsRepository;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<CreatePaymentHandler> _logger;

    public CreatePaymentHandler(
        IAcquiringBankClient bankClient,
        IPaymentsRepository paymentsRepository,
        TimeProvider timeProvider,
        ILogger<CreatePaymentHandler> logger)
    {
        _bankClient = bankClient;
        _paymentsRepository = paymentsRepository;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<CreatePaymentResult> HandleAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var bankResult = await _bankClient.AuthorizeAsync(request, cancellationToken);

            if (bankResult.IsBankUnavailableError)
            {
                _logger.LogWarning("Acquiring bank unavailable, payment not processed");
                return new CreatePaymentResult.BankUnavailableError();
            }

            var payment = Payment.Create(request, bankResult.AsSuccess, _timeProvider.GetUtcNow());

            await _paymentsRepository.AddAsync(payment, cancellationToken);

            _logger.LogInformation("Payment {PaymentId} processed with status {PaymentStatus}", payment.Id, payment.Status);

            return new CreatePaymentResult.Success(payment);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Unexpected error while processing payment");
            return new CreatePaymentResult.InternalError();
        }
    }
}
