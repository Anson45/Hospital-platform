using PharmacyBilling.Domain.Enums;

namespace PharmacyBilling.Application.Payments;

public sealed record GetOutstandingBalancesQuery;

public sealed record OutstandingBalanceDto(
    Guid InvoiceId,
    Guid PatientId,
    Guid VisitId,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal BalanceDue,
    InvoiceStatus Status);
