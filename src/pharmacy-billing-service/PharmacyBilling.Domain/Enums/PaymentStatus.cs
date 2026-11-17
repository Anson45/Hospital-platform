namespace PharmacyBilling.Domain.Enums;

public enum PaymentStatus
{
    Succeeded,

    /// <summary>
    /// Card payments do not reach a gateway in this study project. The status is recorded only.
    /// </summary>
    SimulatedApproved
}
