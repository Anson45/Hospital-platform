using PharmacyBilling.Domain.Enums;

namespace PharmacyBilling.Domain.Entities;

/// <summary>
/// A payment against an invoice. Card payments are recorded with a simulated status only.
/// </summary>
public class Payment
{
    // EF Core materializes rows through this constructor.
    private Payment()
    {
    }

    internal Payment(Guid invoiceId, decimal amount, PaymentMethod method, DateTimeOffset paidAt)
    {
        Id = Guid.NewGuid();
        InvoiceId = invoiceId;
        Amount = amount;
        Method = method;
        Status = method == PaymentMethod.Card ? PaymentStatus.SimulatedApproved : PaymentStatus.Succeeded;
        PaidAt = paidAt;
    }

    public Guid Id { get; }

    public Guid InvoiceId { get; }

    public decimal Amount { get; }

    public PaymentMethod Method { get; }

    public PaymentStatus Status { get; }

    public DateTimeOffset PaidAt { get; }
}
