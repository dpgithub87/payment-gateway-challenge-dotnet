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
            var payment = await CreatePendingPaymentAsync(request, cancellationToken);

            var bankResult = await _bankClient.AuthorizeAsync(request, cancellationToken);

            return bankResult.IsBankUnavailableError
                ? await FailPaymentAsync(payment)
                : await CompletePaymentAsync(payment, bankResult.AsSuccess);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Unexpected error while processing payment");
            return new CreatePaymentResult.InternalError();
        }
    }

    private async Task<Payment> CreatePendingPaymentAsync(CreatePaymentRequest request, CancellationToken cancellationToken)
    {
        var payment = Payment.Create(request, _timeProvider.GetUtcNow());
        await _paymentsRepository.AddAsync(payment, cancellationToken);

        return payment;
    }

    private async Task<CreatePaymentResult> FailPaymentAsync(Payment payment)
    {
        await _paymentsRepository.UpdateAsync(payment.Fail(), CancellationToken.None);
        _logger.LogWarning("Acquiring bank unavailable, payment {PaymentId} failed", payment.Id);

        return new CreatePaymentResult.BankUnavailableError();
    }

    private async Task<CreatePaymentResult> CompletePaymentAsync(Payment payment, BankAuthorizationResult.Success bankResult)
    {
        var completedPayment = payment.Complete(bankResult);
        await _paymentsRepository.UpdateAsync(completedPayment, CancellationToken.None);
        _logger.LogInformation("Payment {PaymentId} processed with status {PaymentStatus}", completedPayment.Id, completedPayment.Status);

        return new CreatePaymentResult.Success(completedPayment);
    }
}
