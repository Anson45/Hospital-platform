using PharmacyBilling.Domain.Entities;

namespace PharmacyBilling.Application.Abstractions;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// The visit's invoice, if one exists. Used so one visit never gets two invoices.
    /// </summary>
    Task<Invoice?> GetByVisitIdAsync(Guid visitId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Invoice>> GetAllAsync(CancellationToken cancellationToken = default);

    void Add(Invoice invoice);
}
