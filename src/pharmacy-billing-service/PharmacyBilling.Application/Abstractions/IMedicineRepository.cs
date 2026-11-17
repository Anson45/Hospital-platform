using PharmacyBilling.Domain.Entities;

namespace PharmacyBilling.Application.Abstractions;

public interface IMedicineRepository
{
    Task<Medicine?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Medicine?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Medicine>> GetAllAsync(CancellationToken cancellationToken = default);

    void Add(Medicine medicine);
}
