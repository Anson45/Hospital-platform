using PharmacyBilling.Domain.Exceptions;

namespace PharmacyBilling.Domain.Entities;

/// <summary>
/// A quantity of one medicine that shares a batch number and expiry date. Only <see cref="Medicine"/> changes it.
/// </summary>
public class MedicineBatch
{
    // EF Core materializes rows through this constructor.
    private MedicineBatch()
    {
        BatchNumber = null!;
    }

    internal MedicineBatch(Guid medicineId, string batchNumber, DateOnly expiryDate)
    {
        Id = Guid.NewGuid();
        MedicineId = medicineId;
        BatchNumber = batchNumber;
        ExpiryDate = expiryDate;
    }

    public Guid Id { get; }

    public Guid MedicineId { get; }

    public string BatchNumber { get; }

    public DateOnly ExpiryDate { get; }

    public int QuantityOnHand { get; private set; }

    internal void Increase(int quantity)
    {
        QuantityOnHand += quantity;
    }

    internal void Decrease(int quantity)
    {
        if (quantity > QuantityOnHand)
        {
            throw new DomainException(
                $"Batch '{BatchNumber}' holds {QuantityOnHand}, which is less than the {quantity} requested.");
        }

        QuantityOnHand -= quantity;
    }
}
