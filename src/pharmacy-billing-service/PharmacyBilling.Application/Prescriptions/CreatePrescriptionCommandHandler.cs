using Microsoft.Extensions.Logging;
using PharmacyBilling.Application.Abstractions;
using PharmacyBilling.Application.Exceptions;
using PharmacyBilling.Domain.Entities;
using PharmacyBilling.Domain.Enums;
using PharmacyBilling.Domain.Exceptions;

namespace PharmacyBilling.Application.Prescriptions;

/// <summary>
/// Issues a prescription for a visit. In one unit of work it dispenses stock, records the prescription,
/// and adds the charges to the visit's invoice. The patient and visit belong to the Patient service and
/// are not checked yet; that check arrives with the gRPC client.
/// </summary>
public sealed partial class CreatePrescriptionCommandHandler : ICommandHandler<CreatePrescriptionCommand, Guid>
{
    private readonly IMedicineRepository _medicines;
    private readonly IInvoiceRepository _invoices;
    private readonly IPrescriptionRepository _prescriptions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<CreatePrescriptionCommandHandler> _logger;

    public CreatePrescriptionCommandHandler(
        IMedicineRepository medicines,
        IInvoiceRepository invoices,
        IPrescriptionRepository prescriptions,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider,
        ILogger<CreatePrescriptionCommandHandler> logger)
    {
        _medicines = medicines;
        _invoices = invoices;
        _prescriptions = prescriptions;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<Guid> HandleAsync(CreatePrescriptionCommand command, CancellationToken cancellationToken = default)
    {
        var now = _timeProvider.GetUtcNow();
        var lines = new List<PrescriptionLine>();

        foreach (var item in command.Items)
        {
            var medicine = await _medicines.GetByIdAsync(item.MedicineId, cancellationToken)
                ?? throw new NotFoundException(nameof(Medicine), item.MedicineId);

            medicine.Dispense(item.Quantity, now);
            lines.Add(new PrescriptionLine(medicine.Id, medicine.Name, item.Quantity, medicine.UnitPrice));
        }

        var prescription = new Prescription(command.PatientId, command.VisitId, lines, now);
        _prescriptions.Add(prescription);

        var invoice = await _invoices.GetByVisitIdAsync(command.VisitId, cancellationToken);
        if (invoice is null)
        {
            invoice = new Invoice(command.PatientId, command.VisitId);
            _invoices.Add(invoice);
        }
        else if (invoice.PatientId != command.PatientId)
        {
            throw new DomainException("The visit's invoice belongs to a different patient.");
        }

        foreach (var line in lines)
        {
            invoice.AddLine(InvoiceLineKind.Prescription, $"{line.MedicineName} x{line.Quantity}", line.LineTotal);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        LogPrescriptionIssued(prescription.Id, command.VisitId, lines.Count, prescription.TotalAmount);

        return prescription.Id;
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Issued prescription {PrescriptionId} for visit {VisitId} with {LineCount} lines totalling {TotalAmount}")]
    private partial void LogPrescriptionIssued(Guid prescriptionId, Guid visitId, int lineCount, decimal totalAmount);
}
