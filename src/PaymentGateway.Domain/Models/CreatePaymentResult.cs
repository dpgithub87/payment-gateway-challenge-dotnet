using OneOf;

using PaymentGateway.Domain.Models;

namespace PaymentGateway.Domain.Models;

[GenerateOneOf]
public partial class CreatePaymentResult : OneOfBase<CreatePaymentResult.Success, CreatePaymentResult.BankUnavailableError, CreatePaymentResult.InternalError>
{
    public sealed record Success(Payment Payment);

    public sealed record BankUnavailableError;

    public sealed record InternalError;

    public bool IsSuccess => IsT0;

    public Success AsSuccess => AsT0;

    public bool IsBankUnavailableError => IsT1;

    public BankUnavailableError AsBankUnavailableError => AsT1;

    public bool IsInternalError => IsT2;

    public InternalError AsInternalError => AsT2;
}
