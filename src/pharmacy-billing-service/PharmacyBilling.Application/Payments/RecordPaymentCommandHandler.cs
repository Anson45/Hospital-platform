using PharmacyBilling.Application.Abstractions;
using PharmacyBilling.Application.Exceptions;
using PharmacyBilling.Domain.Entities;

namespace PharmacyBilling.Application.Payments;

public sealed class RecordPaymentCommandHandler : ICommandHandler<RecordPaymentCommand, Guid>
{
    private readonly IInvoiceRepository _invoices;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public RecordPaymentCommandHandler(IInvoiceRepository invoices, IUnitOfWork unitOfWork, TimeProvider timeProvider)
    {
        _invoices = invoices;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<Guid> HandleAsync(RecordPaymentCommand command, CancellationToken cancellationToken = default)
    {
        var invoice = await _invoices.GetByIdAsync(command.InvoiceId, cancellationToken)
            ?? throw new NotFoundException(nameof(Invoice), command.InvoiceId);

        var payment = invoice.RecordPayment(command.Amount, command.Method, _timeProvider.GetUtcNow());
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return payment.Id;
    }
}
