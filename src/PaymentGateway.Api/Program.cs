using System.Text.Json;
using System.Text.Json.Serialization;

using FluentValidation;

using Microsoft.AspNetCore.HttpLogging;

using PaymentGateway.Api.Constants;
using PaymentGateway.Api.Errors;
using PaymentGateway.Api.Middleware;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Validation;
using PaymentGateway.Domain.Handlers;
using PaymentGateway.Domain.Models;
using PaymentGateway.Domain.Ports.Driving;
using PaymentGateway.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = ProblemDetailsCustomization.UseSnakeCaseTraceId);
builder.Services.AddHealthChecks();
builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.RequestMethod
        | HttpLoggingFields.RequestPath
        | HttpLoggingFields.ResponseStatusCode
        | HttpLoggingFields.Duration;
    options.CombineLogs = true;
});

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IValidator<PostPaymentRequest>, PostPaymentRequestValidator>();
builder.Services.AddScoped<IRequestHandler<CreatePaymentRequest, CreatePaymentResult>, CreatePaymentHandler>();
builder.Services.AddScoped<IRequestHandler<GetPaymentRequest, GetPaymentResult>, GetPaymentHandler>();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseExceptionHandler();
app.UseHttpLogging();

if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Swagger:Enabled"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapHealthChecks(ApiRoutes.HealthChecks.Internal);

app.Run();

public partial class Program
{
}
