using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using PharmacyBilling.Application.Abstractions;
using PharmacyBilling.Application.Exceptions;
using PharmacyBilling.Application.Prescriptions;
using PharmacyBilling.Domain.Entities;
using PharmacyBilling.Domain.Enums;
using PharmacyBilling.Domain.Exceptions;
using PharmacyBilling.Tests.TestDoubles;

namespace PharmacyBilling.Tests.Application.Prescriptions;

public sealed class CreatePrescriptionCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 17, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly FarExpiry = new(2027, 6, 30);

    private readonly IMedicineRepository _medicines = Substitute.For<IMedicineRepository>();
    private readonly IInvoiceRepository _invoices = Substitute.For<IInvoiceRepository>();
    private readonly IPrescriptionRepository _prescriptions = Substitute.For<IPrescriptionRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreatePrescriptionCommandHandler _handler;

    public CreatePrescriptionCommandHandlerTests()
    {
        _handler = new CreatePrescriptionCommandHandler(
            _medicines,
            _invoices,
            _prescriptions,
            _unitOfWork,
            new FixedTimeProvider(Now),
            NullLogger<CreatePrescriptionCommandHandler>.Instance);

        _invoices.GetByVisitIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Invoice?>(null));
    }

    [Fact]
    public async Task HandleAsync_WhenStockIsAvailable_DispensesStockAndAddsChargeToNewInvoice()
    {
        var medicine = CreateMedicineWithStock(unitPrice: 2.50m, quantity: 10);
        SetupMedicine(medicine);
        var visitId = Guid.NewGuid();

        var prescriptionId = await _handler.HandleAsync(
            new CreatePrescriptionCommand(Guid.NewGuid(), visitId, [new PrescriptionItem(medicine.Id, 4)]));

        Assert.NotEqual(Guid.Empty, prescriptionId);
        Assert.Equal(6, medicine.TotalQuantityOnHand);
        _prescriptions.Received(1).Add(Arg.Is<Prescription>(p => p.Id == prescriptionId));
        _invoices.Received(1).Add(Arg.Is<Invoice>(invoice =>
            invoice.VisitId == visitId
            && invoice.TotalAmount == 10m
            && invoice.Lines[0].Kind == InvoiceLineKind.Prescription));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenVisitAlreadyHasInvoice_AddsChargeToExistingInvoice()
    {
        var medicine = CreateMedicineWithStock(unitPrice: 2.50m, quantity: 10);
        SetupMedicine(medicine);
        var patientId = Guid.NewGuid();
        var visitId = Guid.NewGuid();
        var existingInvoice = new Invoice(patientId, visitId);
        existingInvoice.AddLine(InvoiceLineKind.Consultation, "Consultation", 80m);
        _invoices.GetByVisitIdAsync(visitId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Invoice?>(existingInvoice));

        await _handler.HandleAsync(
            new CreatePrescriptionCommand(patientId, visitId, [new PrescriptionItem(medicine.Id, 2)]));

        _invoices.DidNotReceive().Add(Arg.Any<Invoice>());
        Assert.Equal(2, existingInvoice.Lines.Count);
        Assert.Equal(85m, existingInvoice.TotalAmount);
    }

    [Fact]
    public async Task HandleAsync_WhenStockIsInsufficient_ThrowsAndSavesNothing()
    {
        var medicine = CreateMedicineWithStock(unitPrice: 2.50m, quantity: 3);
        SetupMedicine(medicine);

        await Assert.ThrowsAsync<DomainException>(() => _handler.HandleAsync(
            new CreatePrescriptionCommand(Guid.NewGuid(), Guid.NewGuid(), [new PrescriptionItem(medicine.Id, 4)])));

        _prescriptions.DidNotReceive().Add(Arg.Any<Prescription>());
        _invoices.DidNotReceive().Add(Arg.Any<Invoice>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenVisitBelongsToAnotherPatient_Throws()
    {
        var medicine = CreateMedicineWithStock(unitPrice: 2.50m, quantity: 10);
        SetupMedicine(medicine);
        var visitId = Guid.NewGuid();
        var invoiceForOtherPatient = new Invoice(Guid.NewGuid(), visitId);
        _invoices.GetByVisitIdAsync(visitId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Invoice?>(invoiceForOtherPatient));

        await Assert.ThrowsAsync<DomainException>(() => _handler.HandleAsync(
            new CreatePrescriptionCommand(Guid.NewGuid(), visitId, [new PrescriptionItem(medicine.Id, 1)])));

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenMedicineDoesNotExist_ThrowsNotFound()
    {
        var medicineId = Guid.NewGuid();
        _medicines.GetByIdAsync(medicineId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Medicine?>(null));

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.HandleAsync(
            new CreatePrescriptionCommand(Guid.NewGuid(), Guid.NewGuid(), [new PrescriptionItem(medicineId, 1)])));
    }

    private void SetupMedicine(Medicine medicine)
    {
        _medicines.GetByIdAsync(medicine.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Medicine?>(medicine));
    }

    private static Medicine CreateMedicineWithStock(decimal unitPrice, int quantity)
    {
        var medicine = new Medicine("MED-200", "Amoxicillin 500mg", unitPrice);
        medicine.ReceiveStock("B-001", FarExpiry, quantity, Now);
        return medicine;
    }
}
