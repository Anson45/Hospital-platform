using PharmacyBilling.Application.Abstractions;

namespace PharmacyBilling.Application.Inventory;

public sealed class GetExpiryReportQueryHandler : IQueryHandler<GetExpiryReportQuery, IReadOnlyList<ExpiryReportItemDto>>
{
    private readonly IMedicineRepository _medicines;
    private readonly TimeProvider _timeProvider;

    public GetExpiryReportQueryHandler(IMedicineRepository medicines, TimeProvider timeProvider)
    {
        _medicines = medicines;
        _timeProvider = timeProvider;
    }

    public async Task<IReadOnlyList<ExpiryReportItemDto>> HandleAsync(
        GetExpiryReportQuery query,
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);
        var horizon = today.AddDays(query.WithinDays);
        var medicines = await _medicines.GetAllAsync(cancellationToken);

        return medicines
            .SelectMany(medicine => medicine.Batches
                .Where(batch => batch.QuantityOnHand > 0 && batch.ExpiryDate <= horizon)
                .Select(batch => new ExpiryReportItemDto(
                    medicine.Id,
                    medicine.Code,
                    medicine.Name,
                    batch.BatchNumber,
                    batch.ExpiryDate,
                    batch.QuantityOnHand,
                    batch.ExpiryDate < today)))
            .OrderBy(item => item.ExpiryDate)
            .ThenBy(item => item.MedicineName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
