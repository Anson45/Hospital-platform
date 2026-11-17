using Microsoft.AspNetCore.Mvc;
using PharmacyBilling.Application.Abstractions;
using PharmacyBilling.Application.Prescriptions;

namespace PharmacyBilling.Api.Controllers;

[ApiController]
[Route("api/prescriptions")]
public sealed class PrescriptionsController : ControllerBase
{
    private readonly ICommandHandler<CreatePrescriptionCommand, Guid> _create;

    public PrescriptionsController(ICommandHandler<CreatePrescriptionCommand, Guid> create)
    {
        _create = create;
    }

    /// <summary>
    /// Issues a prescription for a visit. Dispenses the stock and adds the charges to the visit's invoice.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreatePrescriptionCommand command, CancellationToken cancellationToken)
    {
        var prescriptionId = await _create.HandleAsync(command, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, prescriptionId);
    }
}
