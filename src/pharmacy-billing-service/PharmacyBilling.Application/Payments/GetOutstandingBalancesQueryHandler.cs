using PharmacyBilling.Application.Abstractions;

namespace PharmacyBilling.Application.Payments;

public sealed class GetOutstandingBalancesQueryHandler
    : IQueryHandler<GetOutstandingBalancesQuery, IReadOnlyList<OutstandingBalanceDto>>
{
    private readonly IInvoiceRepository _invoices;

    public GetOutstandingBalancesQueryHandler(IInvoiceRepository invoices)
    {
        _invoices = invoices;
    }

    public async Task<IReadOnlyList<OutstandingBalanceDto>> HandleAsync(
        GetOutstandingBalancesQuery query,
        CancellationToken cancellationToken = default)
    {
        var invoices = await _invoices.GetAllAsync(cancellationToken);

        return invoices
            .Where(invoice => invoice.BalanceDue > 0)
            .Select(invoice => new OutstandingBalanceDto(
                invoice.Id,
                invoice.PatientId,
                invoice.VisitId,
                invoice.TotalAmount,
                invoice.PaidAmount,
                invoice.BalanceDue,
                invoice.Status))
            .ToList();
    }
}
