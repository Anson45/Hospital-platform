using PharmacyBilling.Domain.Enums;
using PharmacyBilling.Domain.Exceptions;

namespace PharmacyBilling.Domain.Entities;

/// <summary>
/// Charges for one visit. Status is derived from the lines and payments. Adding a charge after full
/// payment reopens the balance, so one invoice per visit is enough.
/// </summary>
public class Invoice
{
    private readonly List<InvoiceLine> _lines = [];
    private readonly List<Payment> _payments = [];

    // EF Core materializes rows through this constructor.
    private Invoice()
    {
    }

    public Invoice(Guid patientId, Guid visitId)
    {
        Id = Guid.NewGuid();
        PatientId = patientId;
        VisitId = visitId;
    }

    public Guid Id { get; }

    public Guid PatientId { get; }

    public Guid VisitId { get; }

    public IReadOnlyList<InvoiceLine> Lines => _lines;

    public IReadOnlyList<Payment> Payments => _payments;

    public decimal TotalAmount => _lines.Sum(line => line.Amount);

    public decimal PaidAmount => _payments.Sum(payment => payment.Amount);

    public decimal BalanceDue => TotalAmount - PaidAmount;

    public InvoiceStatus Status => PaidAmount switch
    {
        0 => InvoiceStatus.Pending,
        _ when BalanceDue == 0 => InvoiceStatus.Paid,
        _ => InvoiceStatus.PartiallyPaid,
    };

    public void AddLine(InvoiceLineKind kind, string description, decimal amount)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentOutOfRangeException.ThrowIfNegative(amount);

        _lines.Add(new InvoiceLine(kind, description, amount));
    }

    public Payment RecordPayment(decimal amount, PaymentMethod method, DateTimeOffset paidAt)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);

        if (BalanceDue == 0)
        {
            throw new DomainException("This invoice has no balance due.");
        }

        if (amount > BalanceDue)
        {
            throw new DomainException($"Payment of {amount} exceeds the balance due of {BalanceDue}.");
        }

        var payment = new Payment(Id, amount, method, paidAt);
        _payments.Add(payment);
        return payment;
    }
}
