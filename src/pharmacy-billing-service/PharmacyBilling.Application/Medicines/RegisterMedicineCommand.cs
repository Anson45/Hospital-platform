namespace PharmacyBilling.Application.Medicines;

public sealed record RegisterMedicineCommand(string Code, string Name, decimal UnitPrice);
