using PharmacyBilling.Domain.Enums;

namespace PharmacyBilling.Domain.Entities;

/// <summary>
/// An immutable record of stock entering or leaving a batch. Movements are the audit trail for stock.
/// </summary>
public class StockMovement
{
    // EF Core materializes rows through this constructor.
    private StockMovement()
    {
    }

    internal StockMovement(
        Guid medicineId,
        Guid batchId,
        int quantity,
        StockMovementType type,
        string? reason,
        DateTimeOffset occurredAt)
    {
        Id = Guid.NewGuid();
        MedicineId = medicineId;
        BatchId = batchId;
        Quantity = quantity;
        Type = type;
        Reason = reason;
        OccurredAt = occurredAt;
    }

    public Guid Id { get; }

    public Guid MedicineId { get; }

    public Guid BatchId { get; }

    /// <summary>
    /// Always positive. <see cref="Type"/> gives the direction.
    /// </summary>
    public int Quantity { get; }

    public StockMovementType Type { get; }

    public string? Reason { get; }

    public DateTimeOffset OccurredAt { get; }
}
