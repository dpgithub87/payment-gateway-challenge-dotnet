using FluentValidation;

using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Constants;
using PaymentGateway.Api.Mappers;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Validation;
using PaymentGateway.Domain.Models;
using PaymentGateway.Domain.Ports.Driving;

namespace PaymentGateway.Api.Controllers;

[ApiController]
[Route(ApiRoutes.Base)]
[Produces("application/json")]
public sealed class PaymentsController : ControllerBase
{
    private readonly IRequestHandler<CreatePaymentRequest, CreatePaymentResult> _createPaymentHandler;
    private readonly IRequestHandler<GetPaymentRequest, GetPaymentResult> _getPaymentHandler;
    private readonly IValidator<PostPaymentRequest> _validator;

    public PaymentsController(
        IRequestHandler<CreatePaymentRequest, CreatePaymentResult> createPaymentHandler,
        IRequestHandler<GetPaymentRequest, GetPaymentResult> getPaymentHandler,
        IValidator<PostPaymentRequest> validator)
    {
        _createPaymentHandler = createPaymentHandler;
        _getPaymentHandler = getPaymentHandler;
        _validator = validator;
    }

    [HttpPost]
    [ProducesResponseType<PaymentResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreatePayment([FromBody] PostPaymentRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            validationResult.AddToModelState(ModelState);
            return ValidationProblem(ModelState);
        }

        var result = await _createPaymentHandler.HandleAsync(request.ToDomain(), cancellationToken);

        return result.Match<IActionResult>(
            success => CreatedAtAction(nameof(GetPayment), new { id = success.Payment.Id }, success.Payment.ToResponse()),
            _ => Problem(statusCode: StatusCodes.Status502BadGateway, title: "Acquiring bank unavailable"),
            _ => Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Unexpected error"));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<PaymentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPayment(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getPaymentHandler.HandleAsync(new GetPaymentRequest(id), cancellationToken);

        return result.Match<IActionResult>(
            success => Ok(success.Payment.ToResponse()),
            _ => Problem(statusCode: StatusCodes.Status404NotFound, title: "Payment not found"));
    }
}
