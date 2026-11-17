namespace PharmacyBilling.Application.Prescriptions;

public sealed record CreatePrescriptionCommand(
    Guid PatientId,
    Guid VisitId,
    IReadOnlyList<PrescriptionItem> Items);

public sealed record PrescriptionItem(Guid MedicineId, int Quantity);
