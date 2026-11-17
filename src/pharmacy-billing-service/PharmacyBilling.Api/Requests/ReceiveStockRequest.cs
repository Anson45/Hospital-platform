namespace PharmacyBilling.Api.Requests;

/// <summary>
/// Body for receiving stock into a medicine. The medicine id comes from the route.
/// </summary>
public sealed record ReceiveStockRequest(string BatchNumber, DateOnly ExpiryDate, int Quantity);
