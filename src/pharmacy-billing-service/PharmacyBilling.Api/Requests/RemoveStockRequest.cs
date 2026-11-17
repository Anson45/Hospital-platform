namespace PharmacyBilling.Api.Requests;

/// <summary>
/// Body for removing stock from one batch. The medicine id comes from the route.
/// </summary>
public sealed record RemoveStockRequest(string BatchNumber, int Quantity, string Reason);
