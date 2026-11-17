using Microsoft.AspNetCore.Mvc;
using PharmacyBilling.Api.Requests;
using PharmacyBilling.Application.Abstractions;
using PharmacyBilling.Application.Medicines;

namespace PharmacyBilling.Api.Controllers;

[ApiController]
[Route("api/medicines")]
public sealed class MedicinesController : ControllerBase
{
    private readonly ICommandHandler<RegisterMedicineCommand, Guid> _register;
    private readonly ICommandHandler<ReceiveStockCommand> _receiveStock;
    private readonly ICommandHandler<RemoveStockCommand> _removeStock;

    public MedicinesController(
        ICommandHandler<RegisterMedicineCommand, Guid> register,
        ICommandHandler<ReceiveStockCommand> receiveStock,
        ICommandHandler<RemoveStockCommand> removeStock)
    {
        _register = register;
        _receiveStock = receiveStock;
        _removeStock = removeStock;
    }

    /// <summary>
    /// Registers a medicine in the catalog. Returns the new medicine id.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Register([FromBody] RegisterMedicineCommand command, CancellationToken cancellationToken)
    {
        var medicineId = await _register.HandleAsync(command, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, medicineId);
    }

    /// <summary>
    /// Adds stock to a batch. A new batch number creates the batch.
    /// </summary>
    [HttpPost("{medicineId:guid}/stock")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ReceiveStock(
        Guid medicineId,
        [FromBody] ReceiveStockRequest request,
        CancellationToken cancellationToken)
    {
        await _receiveStock.HandleAsync(
            new ReceiveStockCommand(medicineId, request.BatchNumber, request.ExpiryDate, request.Quantity),
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Removes stock from one batch, for example after damage or a count correction.
    /// </summary>
    [HttpPost("{medicineId:guid}/stock/removals")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RemoveStock(
        Guid medicineId,
        [FromBody] RemoveStockRequest request,
        CancellationToken cancellationToken)
    {
        await _removeStock.HandleAsync(
            new RemoveStockCommand(medicineId, request.BatchNumber, request.Quantity, request.Reason),
            cancellationToken);

        return NoContent();
    }
}
