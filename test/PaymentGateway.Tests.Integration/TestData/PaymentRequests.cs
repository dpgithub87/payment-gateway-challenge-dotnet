using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Tests.Integration.TestData;

public static class PaymentRequests
{
    public const string AuthorizedCardNumber = "2222405343248877";
    public const string DeclinedCardNumber = "2222405343248112";
    public const string BankUnavailableCardNumber = "2222405343248870";

    public static PostPaymentRequest Valid(string cardNumber = AuthorizedCardNumber)
    {
        var expiry = DateTime.UtcNow.AddYears(1);

        return new PostPaymentRequest(cardNumber, expiry.Month, expiry.Year, "GBP", 100, "123");
    }
}
