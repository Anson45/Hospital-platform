using PharmacyBilling.Domain.Exceptions;

namespace PharmacyBilling.Domain.Entities;

/// <summary>
/// Medicines prescribed during a visit. Issuing a prescription dispenses its stock and produces a charge.
/// </summary>
public class Prescription
{
    private readonly List<PrescriptionLine> _lines = [];

    // EF Core materializes rows through this constructor.
    private Prescription()
    {
    }

    public Prescription(Guid patientId, Guid visitId, IEnumerable<PrescriptionLine> lines, DateTimeOffset issuedAt)
    {
        var lineList = lines.ToList();
        if (lineList.Count == 0)
        {
            throw new DomainException("A prescription needs at least one medicine.");
        }

        if (lineList.Select(line => line.MedicineId).Distinct().Count() != lineList.Count)
        {
            throw new DomainException("A medicine can appear only once in a prescription.");
        }

        Id = Guid.NewGuid();
        PatientId = patientId;
        VisitId = visitId;
        IssuedAt = issuedAt;
        _lines.AddRange(lineList);
    }

    public Guid Id { get; }

    public Guid PatientId { get; }

    public Guid VisitId { get; }

    public DateTimeOffset IssuedAt { get; }

    public IReadOnlyList<PrescriptionLine> Lines => _lines;

    public decimal TotalAmount => _lines.Sum(line => line.LineTotal);
}
