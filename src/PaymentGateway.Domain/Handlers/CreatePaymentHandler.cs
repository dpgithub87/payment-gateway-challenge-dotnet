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
            var payment = Payment.Create(request, _timeProvider.GetUtcNow());
            await _paymentsRepository.AddAsync(payment, cancellationToken);

            var bankResult = await _bankClient.AuthorizeAsync(request, cancellationToken);

            if (bankResult.IsBankUnavailableError)
            {
                await _paymentsRepository.UpdateAsync(payment.Fail(), CancellationToken.None);
                _logger.LogWarning("Acquiring bank unavailable, payment {PaymentId} failed", payment.Id);
                return new CreatePaymentResult.BankUnavailableError();
            }

            var completedPayment = payment.Complete(bankResult.AsSuccess);
            await _paymentsRepository.UpdateAsync(completedPayment, CancellationToken.None);

            _logger.LogInformation("Payment {PaymentId} processed with status {PaymentStatus}", completedPayment.Id, completedPayment.Status);

            return new CreatePaymentResult.Success(completedPayment);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Unexpected error while processing payment");
            return new CreatePaymentResult.InternalError();
        }
    }
}
