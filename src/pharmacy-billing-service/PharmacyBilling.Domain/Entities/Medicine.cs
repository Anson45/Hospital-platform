using PharmacyBilling.Domain.Enums;
using PharmacyBilling.Domain.Exceptions;

namespace PharmacyBilling.Domain.Entities;

/// <summary>
/// Aggregate root for a medicine. It owns its batches and the stock movements that change them,
/// so stock can only change through these methods. Expired batches are never dispensed.
/// </summary>
public class Medicine
{
    private readonly List<MedicineBatch> _batches = [];
    private readonly List<StockMovement> _movements = [];

    // EF Core materializes rows through this constructor.
    private Medicine()
    {
        Code = null!;
        Name = null!;
    }

    public Medicine(string code, string name, decimal unitPrice)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegative(unitPrice);

        Id = Guid.NewGuid();
        Code = code;
        Name = name;
        UnitPrice = unitPrice;
    }

    public Guid Id { get; }

    public string Code { get; }

    public string Name { get; }

    /// <summary>
    /// Current price. Prescriptions copy this value when they are issued, so later price changes do not alter past invoices.
    /// </summary>
    public decimal UnitPrice { get; }

    public IReadOnlyList<MedicineBatch> Batches => _batches;

    public IReadOnlyList<StockMovement> Movements => _movements;

    public int TotalQuantityOnHand => _batches.Sum(batch => batch.QuantityOnHand);

    /// <summary>
    /// Quantity in batches that have not expired as of <paramref name="now"/>.
    /// </summary>
    public int AvailableQuantity(DateTimeOffset now)
    {
        var today = ToDate(now);
        return _batches.Where(batch => IsUsable(batch, today)).Sum(batch => batch.QuantityOnHand);
    }

    /// <summary>
    /// Adds stock to a batch. A new batch number creates a batch. An existing batch number must keep its expiry date.
    /// </summary>
    public void ReceiveStock(string batchNumber, DateOnly expiryDate, int quantity, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(batchNumber);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        if (expiryDate < ToDate(now))
        {
            throw new DomainException("Expired stock cannot be received.");
        }

        var batch = _batches.FirstOrDefault(existing => existing.BatchNumber == batchNumber);
        if (batch is null)
        {
            batch = new MedicineBatch(Id, batchNumber, expiryDate);
            _batches.Add(batch);
        }
        else if (batch.ExpiryDate != expiryDate)
        {
            throw new DomainException($"Batch '{batchNumber}' already exists with a different expiry date.");
        }

        batch.Increase(quantity);
        _movements.Add(new StockMovement(Id, batch.Id, quantity, StockMovementType.Received, null, now));
    }

    /// <summary>
    /// Removes stock from one named batch, for example after damage or a stock count correction.
    /// </summary>
    public void RemoveStock(string batchNumber, int quantity, string reason, DateTimeOffset now)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        var batch = _batches.FirstOrDefault(existing => existing.BatchNumber == batchNumber)
            ?? throw new DomainException($"Batch '{batchNumber}' does not exist for medicine '{Code}'.");

        batch.Decrease(quantity);
        _movements.Add(new StockMovement(Id, batch.Id, quantity, StockMovementType.Removed, reason, now));
    }

    /// <summary>
    /// Takes stock for a prescription. Earliest-expiry batches go first (first expiry, first out).
    /// Returns one movement per batch that was used.
    /// </summary>
    public IReadOnlyList<StockMovement> Dispense(int quantity, DateTimeOffset now)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        var today = ToDate(now);
        var available = _batches.Where(batch => IsUsable(batch, today)).Sum(batch => batch.QuantityOnHand);
        if (available < quantity)
        {
            throw new DomainException(
                $"Insufficient stock for '{Name}'. Requested {quantity}, available {available}.");
        }

        var remaining = quantity;
        var movements = new List<StockMovement>();
        var usableBatches = _batches
            .Where(batch => IsUsable(batch, today) && batch.QuantityOnHand > 0)
            .OrderBy(batch => batch.ExpiryDate)
            .ThenBy(batch => batch.BatchNumber, StringComparer.Ordinal);

        foreach (var batch in usableBatches)
        {
            if (remaining == 0)
            {
                break;
            }

            var taken = Math.Min(remaining, batch.QuantityOnHand);
            batch.Decrease(taken);

            var movement = new StockMovement(Id, batch.Id, taken, StockMovementType.Dispensed, null, now);
            _movements.Add(movement);
            movements.Add(movement);
            remaining -= taken;
        }

        return movements;
    }

    private static bool IsUsable(MedicineBatch batch, DateOnly today) => batch.ExpiryDate >= today;

    private static DateOnly ToDate(DateTimeOffset now) => DateOnly.FromDateTime(now.UtcDateTime);
}
