using PharmacyBilling.Application.Abstractions;
using PharmacyBilling.Application.Exceptions;
using PharmacyBilling.Domain.Entities;

namespace PharmacyBilling.Application.Medicines;

public sealed class ReceiveStockCommandHandler : ICommandHandler<ReceiveStockCommand>
{
    private readonly IMedicineRepository _medicines;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public ReceiveStockCommandHandler(IMedicineRepository medicines, IUnitOfWork unitOfWork, TimeProvider timeProvider)
    {
        _medicines = medicines;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task HandleAsync(ReceiveStockCommand command, CancellationToken cancellationToken = default)
    {
        var medicine = await _medicines.GetByIdAsync(command.MedicineId, cancellationToken)
            ?? throw new NotFoundException(nameof(Medicine), command.MedicineId);

        medicine.ReceiveStock(command.BatchNumber, command.ExpiryDate, command.Quantity, _timeProvider.GetUtcNow());
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
