namespace PharmacyBilling.Application.Inventory;

/// <summary>
/// Lists batches with stock that expire within <see cref="WithinDays"/> days, including batches that have already expired.
/// </summary>
public sealed record GetExpiryReportQuery(int WithinDays = InventoryAlertDefaults.NearExpiryDays);

public sealed record ExpiryReportItemDto(
    Guid MedicineId,
    string MedicineCode,
    string MedicineName,
    string BatchNumber,
    DateOnly ExpiryDate,
    int QuantityOnHand,
    bool IsExpired);
