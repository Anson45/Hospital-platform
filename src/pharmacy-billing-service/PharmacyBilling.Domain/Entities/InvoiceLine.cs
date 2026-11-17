using PharmacyBilling.Domain.Enums;

namespace PharmacyBilling.Domain.Entities;

/// <summary>
/// One charge on an invoice. Amounts are set when the line is added, so later price changes do not affect it.
/// </summary>
public class InvoiceLine
{
    // EF Core materializes rows through this constructor.
    private InvoiceLine()
    {
        Description = null!;
    }

    internal InvoiceLine(InvoiceLineKind kind, string description, decimal amount)
    {
        Id = Guid.NewGuid();
        Kind = kind;
        Description = description;
        Amount = amount;
    }

    public Guid Id { get; }

    public InvoiceLineKind Kind { get; }

    public string Description { get; }

    public decimal Amount { get; }
}
