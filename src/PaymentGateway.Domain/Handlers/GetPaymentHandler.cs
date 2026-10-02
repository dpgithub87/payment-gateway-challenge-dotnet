using Microsoft.Extensions.Logging;

using PaymentGateway.Domain.Models;
using PaymentGateway.Domain.Ports.Driven;
using PaymentGateway.Domain.Ports.Driving;

namespace PaymentGateway.Domain.Handlers;

public sealed class GetPaymentHandler : IRequestHandler<GetPaymentRequest, GetPaymentResult>
{
    private readonly IPaymentsRepository _paymentsRepository;
    private readonly ILogger<GetPaymentHandler> _logger;

    public GetPaymentHandler(IPaymentsRepository paymentsRepository, ILogger<GetPaymentHandler> logger)
    {
        _paymentsRepository = paymentsRepository;
        _logger = logger;
    }

    public async Task<GetPaymentResult> HandleAsync(GetPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var payment = await _paymentsRepository.GetAsync(request.Id, cancellationToken);

        if (payment is null)
        {
            _logger.LogInformation("Payment {PaymentId} not found", request.Id);
            return new GetPaymentResult.NotFound();
        }

        return new GetPaymentResult.Success(payment);
    }
}
