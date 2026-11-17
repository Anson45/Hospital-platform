using PharmacyBilling.Domain.Entities;

namespace PharmacyBilling.Application.Abstractions;

public interface IPrescriptionRepository
{
    void Add(Prescription prescription);
}
