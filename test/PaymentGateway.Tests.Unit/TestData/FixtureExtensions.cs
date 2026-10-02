using AutoFixture;

using PaymentGateway.Api.Models.Requests;

using PaymentGateway.Domain.Enums;
using PaymentGateway.Domain.Models;

namespace PaymentGateway.Tests.Unit.TestData;

public static class FixtureExtensions
{
    public static CreatePaymentRequest CreatePaymentRequest(this IFixture fixture)
    {
        return fixture.Build<CreatePaymentRequest>()
            .With(r => r.CardNumber, "2222405343248877")
            .With(r => r.ExpiryMonth, 4)
            .With(r => r.ExpiryYear, 2027)
            .With(r => r.Currency, "GBP")
            .With(r => r.Amount, 100)
            .With(r => r.Cvv, "123")
            .Create();
    }

    public static PostPaymentRequest CreatePostPaymentRequest(this IFixture fixture)
    {
        return fixture.Build<PostPaymentRequest>()
            .With(r => r.CardNumber, "2222405343248877")
            .With(r => r.ExpiryMonth, 4)
            .With(r => r.ExpiryYear, 2027)
            .With(r => r.Currency, "GBP")
            .With(r => r.Amount, 100)
            .With(r => r.Cvv, "123")
            .Create();
    }

    public static Payment CreatePayment(this IFixture fixture)
    {
        return fixture.Build<Payment>()
            .With(p => p.Status, PaymentStatus.Authorized)
            .With(p => p.CardNumberLastFour, "8877")
            .With(p => p.ExpiryMonth, 4)
            .With(p => p.ExpiryYear, 2027)
            .With(p => p.Currency, "GBP")
            .With(p => p.Amount, 100)
            .With(p => p.AuthorizationCode, fixture.Create<Guid>().ToString())
            .Create();
    }
}
