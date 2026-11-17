using System.Text.Json.Serialization;
using Microsoft.OpenApi;
using PharmacyBilling.Api.ExceptionHandling;
using PharmacyBilling.Application.Abstractions;
using PharmacyBilling.Application.Inventory;
using PharmacyBilling.Application.Medicines;
using PharmacyBilling.Application.Payments;
using PharmacyBilling.Application.Prescriptions;
using PharmacyBilling.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Pharmacy & Billing API",
        Version = "v1",
        Description = "Medicines, stock, prescriptions, and invoices. Synthetic data only. Not for clinical use."
    });
});

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApplicationExceptionHandler>();
builder.Services.AddHealthChecks();

// Handlers read the current time through this abstraction so tests can control it.
builder.Services.AddSingleton(TimeProvider.System);

var connectionString = builder.Configuration.GetConnectionString("PharmacyBilling")
    ?? throw new InvalidOperationException("Connection string 'PharmacyBilling' is not configured.");
builder.Services.AddInfrastructure(connectionString);

builder.Services.AddScoped<ICommandHandler<RegisterMedicineCommand, Guid>, RegisterMedicineCommandHandler>();
builder.Services.AddScoped<ICommandHandler<ReceiveStockCommand>, ReceiveStockCommandHandler>();
builder.Services.AddScoped<ICommandHandler<RemoveStockCommand>, RemoveStockCommandHandler>();
builder.Services.AddScoped<ICommandHandler<CreatePrescriptionCommand, Guid>, CreatePrescriptionCommandHandler>();
builder.Services.AddScoped<ICommandHandler<RecordPaymentCommand, Guid>, RecordPaymentCommandHandler>();
builder.Services.AddScoped<
    IQueryHandler<GetStockLevelsQuery, IReadOnlyList<StockLevelDto>>,
    GetStockLevelsQueryHandler>();
builder.Services.AddScoped<
    IQueryHandler<GetExpiryReportQuery, IReadOnlyList<ExpiryReportItemDto>>,
    GetExpiryReportQueryHandler>();
builder.Services.AddScoped<
    IQueryHandler<GetOutstandingBalancesQuery, IReadOnlyList<OutstandingBalanceDto>>,
    GetOutstandingBalancesQueryHandler>();

var app = builder.Build();

// Swagger UI is for development only, so the API surface is not published by default.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
