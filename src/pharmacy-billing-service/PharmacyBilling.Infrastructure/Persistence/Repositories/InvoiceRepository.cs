using Microsoft.EntityFrameworkCore;
using PharmacyBilling.Application.Abstractions;
using PharmacyBilling.Domain.Entities;

namespace PharmacyBilling.Infrastructure.Persistence.Repositories;

public sealed class InvoiceRepository : IInvoiceRepository
{
    private readonly PharmacyBillingDbContext _context;

    public InvoiceRepository(PharmacyBillingDbContext context)
    {
        _context = context;
    }

    public Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return WithCharges(_context.Invoices).FirstOrDefaultAsync(invoice => invoice.Id == id, cancellationToken);
    }

    public Task<Invoice?> GetByVisitIdAsync(Guid visitId, CancellationToken cancellationToken = default)
    {
        return WithCharges(_context.Invoices).FirstOrDefaultAsync(invoice => invoice.VisitId == visitId, cancellationToken);
    }

    public async Task<IReadOnlyList<Invoice>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await WithCharges(_context.Invoices).ToListAsync(cancellationToken);
    }

    public void Add(Invoice invoice)
    {
        _context.Invoices.Add(invoice);
    }

    private static IQueryable<Invoice> WithCharges(IQueryable<Invoice> invoices)
    {
        return invoices
            .Include(invoice => invoice.Lines)
            .Include(invoice => invoice.Payments);
    }
}
