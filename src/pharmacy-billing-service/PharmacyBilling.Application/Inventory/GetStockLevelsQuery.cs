namespace PharmacyBilling.Application.Inventory;

public sealed record GetStockLevelsQuery(int LowStockThreshold = InventoryAlertDefaults.LowStockThreshold);

public sealed record StockLevelDto(
    Guid MedicineId,
    string Code,
    string Name,
    int TotalQuantity,
    int AvailableQuantity,
    bool IsLowStock);
