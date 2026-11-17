using NSubstitute;
using PharmacyBilling.Application.Abstractions;
using PharmacyBilling.Application.Exceptions;
using PharmacyBilling.Application.Payments;
using PharmacyBilling.Domain.Entities;
using PharmacyBilling.Domain.Enums;
using PharmacyBilling.Domain.Exceptions;
using PharmacyBilling.Tests.TestDoubles;

namespace PharmacyBilling.Tests.Application.Payments;

public sealed class RecordPaymentCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 17, 10, 0, 0, TimeSpan.Zero);

    private readonly IInvoiceRepository _invoices = Substitute.For<IInvoiceRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly RecordPaymentCommandHandler _handler;

    public RecordPaymentCommandHandlerTests()
    {
        _handler = new RecordPaymentCommandHandler(_invoices, _unitOfWork, new FixedTimeProvider(Now));
    }

    [Fact]
    public async Task HandleAsync_WhenAmountIsValid_RecordsPaymentAndReturnsItsId()
    {
        var invoice = CreateInvoice(total: 100m);
        SetupInvoice(invoice);

        var paymentId = await _handler.HandleAsync(
            new RecordPaymentCommand(invoice.Id, 60m, PaymentMethod.Cash));

        Assert.Equal(paymentId, invoice.Payments.Single().Id);
        Assert.Equal(40m, invoice.BalanceDue);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAmountExceedsBalance_ThrowsAndSavesNothing()
    {
        var invoice = CreateInvoice(total: 100m);
        SetupInvoice(invoice);

        await Assert.ThrowsAsync<DomainException>(() =>
            _handler.HandleAsync(new RecordPaymentCommand(invoice.Id, 150m, PaymentMethod.Card)));

        Assert.Empty(invoice.Payments);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenInvoiceDoesNotExist_ThrowsNotFound()
    {
        var invoiceId = Guid.NewGuid();
        _invoices.GetByIdAsync(invoiceId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Invoice?>(null));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.HandleAsync(new RecordPaymentCommand(invoiceId, 10m, PaymentMethod.Cash)));
    }

    private void SetupInvoice(Invoice invoice)
    {
        _invoices.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Invoice?>(invoice));
    }

    private static Invoice CreateInvoice(decimal total)
    {
        var invoice = new Invoice(Guid.NewGuid(), Guid.NewGuid());
        invoice.AddLine(InvoiceLineKind.Consultation, "Consultation", total);
        return invoice;
    }
}
