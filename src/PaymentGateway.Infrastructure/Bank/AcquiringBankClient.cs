using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.Extensions.Logging;

using PaymentGateway.Domain.Models;
using PaymentGateway.Domain.Ports.Driven;
using PaymentGateway.Infrastructure.Bank.Mappers;
using PaymentGateway.Infrastructure.Bank.Models;

namespace PaymentGateway.Infrastructure.Bank;

public sealed class AcquiringBankClient : IAcquiringBankClient
{
    private const string PaymentsPath = "payments";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private readonly HttpClient _httpClient;
    private readonly ILogger<AcquiringBankClient> _logger;

    public AcquiringBankClient(HttpClient httpClient, ILogger<AcquiringBankClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<BankAuthorizationResult> AuthorizeAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.PostAsJsonAsync(PaymentsPath, request.ToBankPaymentRequest(), JsonOptions, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Acquiring bank returned {StatusCode}", (int)response.StatusCode);
                return new BankAuthorizationResult.BankUnavailableError();
            }

            var bankResponse = await response.Content.ReadFromJsonAsync<BankPaymentResponse>(JsonOptions, cancellationToken);

            if (bankResponse is not null)
            {
                return bankResponse.ToDomain();
            }

            _logger.LogWarning("Acquiring bank returned an empty response");
            return new BankAuthorizationResult.BankUnavailableError();

        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Acquiring bank request failed");
            return new BankAuthorizationResult.BankUnavailableError();
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "Acquiring bank request timed out");
            return new BankAuthorizationResult.BankUnavailableError();
        }
    }
}
