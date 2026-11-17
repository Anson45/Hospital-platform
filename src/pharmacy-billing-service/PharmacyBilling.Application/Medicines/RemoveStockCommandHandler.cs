using PharmacyBilling.Application.Abstractions;
using PharmacyBilling.Application.Exceptions;
using PharmacyBilling.Domain.Entities;

namespace PharmacyBilling.Application.Medicines;

public sealed class RemoveStockCommandHandler : ICommandHandler<RemoveStockCommand>
{
    private readonly IMedicineRepository _medicines;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public RemoveStockCommandHandler(IMedicineRepository medicines, IUnitOfWork unitOfWork, TimeProvider timeProvider)
    {
        _medicines = medicines;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task HandleAsync(RemoveStockCommand command, CancellationToken cancellationToken = default)
    {
        var medicine = await _medicines.GetByIdAsync(command.MedicineId, cancellationToken)
            ?? throw new NotFoundException(nameof(Medicine), command.MedicineId);

        medicine.RemoveStock(command.BatchNumber, command.Quantity, command.Reason, _timeProvider.GetUtcNow());
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
