namespace PharmacyBilling.Application.Medicines;

public sealed record RemoveStockCommand(
    Guid MedicineId,
    string BatchNumber,
    int Quantity,
    string Reason);
