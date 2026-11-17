namespace PharmacyBilling.Application.Inventory;

/// <summary>
/// Default alert thresholds. Queries accept overrides, and the API will bind them from configuration later.
/// </summary>
public static class InventoryAlertDefaults
{
    /// <summary>
    /// Available quantity below this value counts as low stock.
    /// </summary>
    public const int LowStockThreshold = 20;

    /// <summary>
    /// A batch that expires within this many days counts as near expiry.
    /// </summary>
    public const int NearExpiryDays = 30;
}
