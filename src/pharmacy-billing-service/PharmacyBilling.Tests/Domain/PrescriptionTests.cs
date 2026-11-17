using PharmacyBilling.Domain.Entities;
using PharmacyBilling.Domain.Exceptions;

namespace PharmacyBilling.Tests.Domain;

public sealed class PrescriptionTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 17, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Constructor_WhenNoLines_Throws()
    {
        Assert.Throws<DomainException>(() =>
            new Prescription(Guid.NewGuid(), Guid.NewGuid(), [], Now));
    }

    [Fact]
    public void Constructor_WhenSameMedicineAppearsTwice_Throws()
    {
        var medicineId = Guid.NewGuid();
        var lines = new[]
        {
            new PrescriptionLine(medicineId, "Paracetamol", 1, 1m),
            new PrescriptionLine(medicineId, "Paracetamol", 2, 1m),
        };

        Assert.Throws<DomainException>(() =>
            new Prescription(Guid.NewGuid(), Guid.NewGuid(), lines, Now));
    }

    [Fact]
    public void TotalAmount_SumsLineTotals()
    {
        var lines = new[]
        {
            new PrescriptionLine(Guid.NewGuid(), "Amoxicillin", 2, 3.50m),
            new PrescriptionLine(Guid.NewGuid(), "Ibuprofen", 3, 1.25m),
        };

        var prescription = new Prescription(Guid.NewGuid(), Guid.NewGuid(), lines, Now);

        Assert.Equal(10.75m, prescription.TotalAmount);
    }
}
