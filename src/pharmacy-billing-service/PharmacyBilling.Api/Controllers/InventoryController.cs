using Microsoft.AspNetCore.Mvc;
using PharmacyBilling.Application.Abstractions;
using PharmacyBilling.Application.Inventory;

namespace PharmacyBilling.Api.Controllers;

[ApiController]
[Route("api/inventory")]
public sealed class InventoryController : ControllerBase
{
    private readonly IQueryHandler<GetStockLevelsQuery, IReadOnlyList<StockLevelDto>> _stockLevels;
    private readonly IQueryHandler<GetExpiryReportQuery, IReadOnlyList<ExpiryReportItemDto>> _expiryReport;

    public InventoryController(
        IQueryHandler<GetStockLevelsQuery, IReadOnlyList<StockLevelDto>> stockLevels,
        IQueryHandler<GetExpiryReportQuery, IReadOnlyList<ExpiryReportItemDto>> expiryReport)
    {
        _stockLevels = stockLevels;
        _expiryReport = expiryReport;
    }

    /// <summary>
    /// Stock per medicine. A medicine is flagged low stock when its available quantity is below the threshold.
    /// </summary>
    [HttpGet("stock-levels")]
    [ProducesResponseType<IReadOnlyList<StockLevelDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStockLevels(
        [FromQuery] int lowStockThreshold = InventoryAlertDefaults.LowStockThreshold,
        CancellationToken cancellationToken = default)
    {
        var levels = await _stockLevels.HandleAsync(new GetStockLevelsQuery(lowStockThreshold), cancellationToken);

        return Ok(levels);
    }

    /// <summary>
    /// Batches with stock that expire within the given number of days, including batches already expired.
    /// </summary>
    [HttpGet("expiry-report")]
    [ProducesResponseType<IReadOnlyList<ExpiryReportItemDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExpiryReport(
        [FromQuery] int withinDays = InventoryAlertDefaults.NearExpiryDays,
        CancellationToken cancellationToken = default)
    {
        var report = await _expiryReport.HandleAsync(new GetExpiryReportQuery(withinDays), cancellationToken);

        return Ok(report);
    }
}
