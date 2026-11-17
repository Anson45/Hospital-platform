using PharmacyBilling.Domain.Exceptions;

namespace PharmacyBilling.Domain.Entities;

/// <summary>
/// One medicine on a prescription. Keeps the unit price from the time the prescription was issued.
/// </summary>
public class PrescriptionLine
{
    // EF Core materializes rows through this constructor.
    private PrescriptionLine()
    {
        MedicineName = null!;
    }

    public PrescriptionLine(Guid medicineId, string medicineName, int quantity, decimal unitPrice)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(medicineName);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        ArgumentOutOfRangeException.ThrowIfNegative(unitPrice);

        Id = Guid.NewGuid();
        MedicineId = medicineId;
        MedicineName = medicineName;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public Guid Id { get; }

    public Guid MedicineId { get; }

    public string MedicineName { get; }

    public int Quantity { get; }

    public decimal UnitPrice { get; }

    public decimal LineTotal => Quantity * UnitPrice;
}
