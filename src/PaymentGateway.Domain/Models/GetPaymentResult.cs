using OneOf;

using PaymentGateway.Domain.Models;

namespace PaymentGateway.Domain.Models;

[GenerateOneOf]
public partial class GetPaymentResult : OneOfBase<GetPaymentResult.Success, GetPaymentResult.NotFound>
{
    public sealed record Success(Payment Payment);

    public sealed record NotFound;

    public bool IsSuccess => IsT0;

    public Success AsSuccess => AsT0;

    public bool IsNotFound => IsT1;

    public NotFound AsNotFound => AsT1;
}
