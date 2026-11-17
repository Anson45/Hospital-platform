using PharmacyBilling.Domain.Enums;

namespace PharmacyBilling.Application.Payments;

public sealed record RecordPaymentCommand(Guid InvoiceId, decimal Amount, PaymentMethod Method);
