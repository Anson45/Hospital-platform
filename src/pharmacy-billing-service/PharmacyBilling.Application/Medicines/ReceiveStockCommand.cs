namespace PharmacyBilling.Application.Medicines;

public sealed record ReceiveStockCommand(
    Guid MedicineId,
    string BatchNumber,
    DateOnly ExpiryDate,
    int Quantity);
