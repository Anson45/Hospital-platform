using PharmacyBilling.Domain.Entities;
using PharmacyBilling.Domain.Enums;
using PharmacyBilling.Domain.Exceptions;

namespace PharmacyBilling.Tests.Domain;

public sealed class InvoiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 17, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void RecordPayment_WhenPartial_SetsPartiallyPaid()
    {
        var invoice = CreateInvoiceWithCharge(100m);

        invoice.RecordPayment(40m, PaymentMethod.Cash, Now);

        Assert.Equal(InvoiceStatus.PartiallyPaid, invoice.Status);
        Assert.Equal(60m, invoice.BalanceDue);
    }

    [Fact]
    public void RecordPayment_WhenCoversBalance_SetsPaid()
    {
        var invoice = CreateInvoiceWithCharge(100m);

        invoice.RecordPayment(100m, PaymentMethod.Cash, Now);

        Assert.Equal(InvoiceStatus.Paid, invoice.Status);
        Assert.Equal(0m, invoice.BalanceDue);
    }

    [Fact]
    public void RecordPayment_WhenAmountExceedsBalance_Throws()
    {
        var invoice = CreateInvoiceWithCharge(100m);

        Assert.Throws<DomainException>(() => invoice.RecordPayment(100.01m, PaymentMethod.Cash, Now));
    }

    [Fact]
    public void RecordPayment_WhenNothingIsOwed_Throws()
    {
        var invoice = new Invoice(Guid.NewGuid(), Guid.NewGuid());

        Assert.Throws<DomainException>(() => invoice.RecordPayment(10m, PaymentMethod.Cash, Now));
    }

    [Fact]
    public void RecordPayment_WhenCard_RecordsSimulatedApprovedStatus()
    {
        var invoice = CreateInvoiceWithCharge(100m);

        var payment = invoice.RecordPayment(25m, PaymentMethod.Card, Now);

        Assert.Equal(PaymentStatus.SimulatedApproved, payment.Status);
    }

    [Fact]
    public void RecordPayment_WhenCash_RecordsSucceededStatus()
    {
        var invoice = CreateInvoiceWithCharge(100m);

        var payment = invoice.RecordPayment(25m, PaymentMethod.Cash, Now);

        Assert.Equal(PaymentStatus.Succeeded, payment.Status);
    }

    [Fact]
    public void AddLine_AfterFullPayment_ReopensBalance()
    {
        var invoice = CreateInvoiceWithCharge(100m);
        invoice.RecordPayment(100m, PaymentMethod.Cash, Now);

        invoice.AddLine(InvoiceLineKind.Prescription, "Ibuprofen x2", 50m);

        Assert.Equal(50m, invoice.BalanceDue);
        Assert.Equal(InvoiceStatus.PartiallyPaid, invoice.Status);
    }

    [Fact]
    public void AddLine_WhenAmountIsNegative_Throws()
    {
        var invoice = new Invoice(Guid.NewGuid(), Guid.NewGuid());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            invoice.AddLine(InvoiceLineKind.Consultation, "Consultation", -1m));
    }

    private static Invoice CreateInvoiceWithCharge(decimal amount)
    {
        var invoice = new Invoice(Guid.NewGuid(), Guid.NewGuid());
        invoice.AddLine(InvoiceLineKind.Consultation, "Consultation", amount);
        return invoice;
    }
}
