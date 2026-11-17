using PharmacyBilling.Application.Abstractions;
using PharmacyBilling.Domain.Entities;

namespace PharmacyBilling.Infrastructure.Persistence.Repositories;

public sealed class PrescriptionRepository : IPrescriptionRepository
{
    private readonly PharmacyBillingDbContext _context;

    public PrescriptionRepository(PharmacyBillingDbContext context)
    {
        _context = context;
    }

    public void Add(Prescription prescription)
    {
        _context.Prescriptions.Add(prescription);
    }
}
