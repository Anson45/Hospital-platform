using PharmacyBilling.Application.Abstractions;
using PharmacyBilling.Domain.Entities;
using PharmacyBilling.Domain.Exceptions;

namespace PharmacyBilling.Application.Medicines;

public sealed class RegisterMedicineCommandHandler : ICommandHandler<RegisterMedicineCommand, Guid>
{
    private readonly IMedicineRepository _medicines;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterMedicineCommandHandler(IMedicineRepository medicines, IUnitOfWork unitOfWork)
    {
        _medicines = medicines;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> HandleAsync(RegisterMedicineCommand command, CancellationToken cancellationToken = default)
    {
        var existing = await _medicines.GetByCodeAsync(command.Code, cancellationToken);
        if (existing is not null)
        {
            throw new DomainException($"A medicine with code '{command.Code}' is already registered.");
        }

        var medicine = new Medicine(command.Code, command.Name, command.UnitPrice);
        _medicines.Add(medicine);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return medicine.Id;
    }
}
