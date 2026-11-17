using PharmacyBilling.Application.Abstractions;

namespace PharmacyBilling.Application.Inventory;

public sealed class GetStockLevelsQueryHandler : IQueryHandler<GetStockLevelsQuery, IReadOnlyList<StockLevelDto>>
{
    private readonly IMedicineRepository _medicines;
    private readonly TimeProvider _timeProvider;

    public GetStockLevelsQueryHandler(IMedicineRepository medicines, TimeProvider timeProvider)
    {
        _medicines = medicines;
        _timeProvider = timeProvider;
    }

    public async Task<IReadOnlyList<StockLevelDto>> HandleAsync(
        GetStockLevelsQuery query,
        CancellationToken cancellationToken = default)
    {
        var now = _timeProvider.GetUtcNow();
        var medicines = await _medicines.GetAllAsync(cancellationToken);

        return medicines
            .OrderBy(medicine => medicine.Name, StringComparer.OrdinalIgnoreCase)
            .Select(medicine =>
            {
                var available = medicine.AvailableQuantity(now);
                return new StockLevelDto(
                    medicine.Id,
                    medicine.Code,
                    medicine.Name,
                    medicine.TotalQuantityOnHand,
                    available,
                    available < query.LowStockThreshold);
            })
            .ToList();
    }
}
