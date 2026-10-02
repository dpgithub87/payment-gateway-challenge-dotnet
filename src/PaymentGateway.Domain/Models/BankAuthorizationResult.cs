using OneOf;

namespace PaymentGateway.Domain.Models;

[GenerateOneOf]
public partial class BankAuthorizationResult : OneOfBase<BankAuthorizationResult.Success, BankAuthorizationResult.BankUnavailableError>
{
    public sealed record Success(bool Authorized, string? AuthorizationCode);

    public sealed record BankUnavailableError;

    public bool IsSuccess => IsT0;

    public Success AsSuccess => AsT0;

    public bool IsBankUnavailableError => IsT1;

    public BankUnavailableError AsBankUnavailableError => AsT1;
}
