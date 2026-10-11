namespace PaymentGateway.Infrastructure.Bank;

public sealed class AcquiringBankOptions
{
    public const string SectionName = "AcquiringBank";

    public string BaseUrl { get; set; } = null!;

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);
}
