using PharmacyBilling.Domain.Enums;

namespace PharmacyBilling.Api.Requests;

/// <summary>
/// Body for recording a payment. The invoice id comes from the route.
/// </summary>
public sealed record RecordPaymentRequest(decimal Amount, PaymentMethod Method);
