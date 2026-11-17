using Microsoft.EntityFrameworkCore;
using PharmacyBilling.Application.Abstractions;
using PharmacyBilling.Domain.Entities;

namespace PharmacyBilling.Infrastructure.Persistence.Repositories;

public sealed class MedicineRepository : IMedicineRepository
{
    private readonly PharmacyBillingDbContext _context;

    public MedicineRepository(PharmacyBillingDbContext context)
    {
        _context = context;
    }

    public Task<Medicine?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return WithStock(_context.Medicines).FirstOrDefaultAsync(medicine => medicine.Id == id, cancellationToken);
    }

    public Task<Medicine?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return WithStock(_context.Medicines).FirstOrDefaultAsync(medicine => medicine.Code == code, cancellationToken);
    }

    public async Task<IReadOnlyList<Medicine>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await WithStock(_context.Medicines).ToListAsync(cancellationToken);
    }

    public void Add(Medicine medicine)
    {
        _context.Medicines.Add(medicine);
    }

    // Batches and movements are loaded with the medicine because the aggregate needs them to enforce its rules.
    private static IQueryable<Medicine> WithStock(IQueryable<Medicine> medicines)
    {
        return medicines
            .Include(medicine => medicine.Batches)
            .Include(medicine => medicine.Movements);
    }
}
