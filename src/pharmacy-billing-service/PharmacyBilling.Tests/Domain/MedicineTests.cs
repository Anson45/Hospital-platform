using PharmacyBilling.Domain.Entities;
using PharmacyBilling.Domain.Enums;
using PharmacyBilling.Domain.Exceptions;

namespace PharmacyBilling.Tests.Domain;

public sealed class MedicineTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 17, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly FarExpiry = new(2027, 6, 30);

    [Fact]
    public void ReceiveStock_WhenBatchIsNew_AddsBatchAndReceivedMovement()
    {
        var medicine = CreateMedicine();

        medicine.ReceiveStock("B-001", FarExpiry, 50, Now);

        Assert.Equal(50, medicine.TotalQuantityOnHand);
        Assert.Single(medicine.Batches);
        Assert.Single(medicine.Movements);
        Assert.Equal(StockMovementType.Received, medicine.Movements[0].Type);
    }

    [Fact]
    public void ReceiveStock_WhenSameBatchHasDifferentExpiry_Throws()
    {
        var medicine = CreateMedicine();
        medicine.ReceiveStock("B-001", FarExpiry, 50, Now);

        Assert.Throws<DomainException>(() => medicine.ReceiveStock("B-001", FarExpiry.AddMonths(1), 10, Now));
    }

    [Fact]
    public void ReceiveStock_WhenExpiryIsInThePast_Throws()
    {
        var medicine = CreateMedicine();

        Assert.Throws<DomainException>(() =>
            medicine.ReceiveStock("B-OLD", new DateOnly(2026, 11, 1), 10, Now));
    }

    [Fact]
    public void ReceiveStock_WhenQuantityIsZero_Throws()
    {
        var medicine = CreateMedicine();

        Assert.Throws<ArgumentOutOfRangeException>(() => medicine.ReceiveStock("B-001", FarExpiry, 0, Now));
    }

    [Fact]
    public void Dispense_WhenSeveralBatchesExist_TakesEarliestExpiryFirst()
    {
        var medicine = CreateMedicine();
        medicine.ReceiveStock("LATE", new DateOnly(2027, 12, 31), 20, Now);
        medicine.ReceiveStock("EARLY", new DateOnly(2027, 1, 31), 20, Now);

        medicine.Dispense(15, Now);

        Assert.Equal(5, GetBatch(medicine, "EARLY").QuantityOnHand);
        Assert.Equal(20, GetBatch(medicine, "LATE").QuantityOnHand);
    }

    [Fact]
    public void Dispense_WhenStockSpansBatches_RecordsOneMovementPerBatchUsed()
    {
        var medicine = CreateMedicine();
        medicine.ReceiveStock("EARLY", new DateOnly(2027, 1, 31), 10, Now);
        medicine.ReceiveStock("LATE", new DateOnly(2027, 12, 31), 10, Now);

        var movements = medicine.Dispense(14, Now);

        Assert.Equal(2, movements.Count);
        Assert.All(movements, movement => Assert.Equal(StockMovementType.Dispensed, movement.Type));
        Assert.Equal(14, movements.Sum(movement => movement.Quantity));
    }

    [Fact]
    public void Dispense_WhenOnlyExpiredStockRemains_Throws()
    {
        var medicine = CreateMedicine();
        medicine.ReceiveStock("SOON", new DateOnly(2026, 11, 20), 10, Now);
        var later = new DateTimeOffset(2026, 11, 25, 10, 0, 0, TimeSpan.Zero);

        Assert.Throws<DomainException>(() => medicine.Dispense(1, later));
    }

    [Fact]
    public void Dispense_WhenStockIsInsufficient_ThrowsAndLeavesStockUnchanged()
    {
        var medicine = CreateMedicine();
        medicine.ReceiveStock("B-001", FarExpiry, 5, Now);

        Assert.Throws<DomainException>(() => medicine.Dispense(6, Now));

        Assert.Equal(5, medicine.TotalQuantityOnHand);
        Assert.Single(medicine.Movements);
    }

    [Fact]
    public void RemoveStock_WhenBatchDoesNotExist_Throws()
    {
        var medicine = CreateMedicine();

        Assert.Throws<DomainException>(() => medicine.RemoveStock("MISSING", 1, "Count correction", Now));
    }

    [Fact]
    public void RemoveStock_WhenQuantityExceedsBatch_Throws()
    {
        var medicine = CreateMedicine();
        medicine.ReceiveStock("B-001", FarExpiry, 3, Now);

        Assert.Throws<DomainException>(() => medicine.RemoveStock("B-001", 4, "Damaged", Now));
    }

    [Fact]
    public void RemoveStock_WhenValid_ReducesBatchAndRecordsReason()
    {
        var medicine = CreateMedicine();
        medicine.ReceiveStock("B-001", FarExpiry, 10, Now);

        medicine.RemoveStock("B-001", 4, "Damaged in transit", Now);

        Assert.Equal(6, medicine.TotalQuantityOnHand);
        var removal = medicine.Movements[^1];
        Assert.Equal(StockMovementType.Removed, removal.Type);
        Assert.Equal("Damaged in transit", removal.Reason);
    }

    private static Medicine CreateMedicine() => new("MED-100", "Amoxicillin 500mg", 2.50m);

    private static MedicineBatch GetBatch(Medicine medicine, string batchNumber) =>
        medicine.Batches.Single(batch => batch.BatchNumber == batchNumber);
}
