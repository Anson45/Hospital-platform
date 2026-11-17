using Microsoft.AspNetCore.Mvc;
using PharmacyBilling.Api.Requests;
using PharmacyBilling.Application.Abstractions;
using PharmacyBilling.Application.Payments;

namespace PharmacyBilling.Api.Controllers;

[ApiController]
[Route("api/invoices")]
public sealed class InvoicesController : ControllerBase
{
    private readonly ICommandHandler<RecordPaymentCommand, Guid> _recordPayment;
    private readonly IQueryHandler<GetOutstandingBalancesQuery, IReadOnlyList<OutstandingBalanceDto>> _outstandingBalances;

    public InvoicesController(
        ICommandHandler<RecordPaymentCommand, Guid> recordPayment,
        IQueryHandler<GetOutstandingBalancesQuery, IReadOnlyList<OutstandingBalanceDto>> outstandingBalances)
    {
        _recordPayment = recordPayment;
        _outstandingBalances = outstandingBalances;
    }

    /// <summary>
    /// Lists invoices that still have a balance due.
    /// </summary>
    [HttpGet("outstanding-balances")]
    [ProducesResponseType<IReadOnlyList<OutstandingBalanceDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOutstandingBalances(CancellationToken cancellationToken)
    {
        var balances = await _outstandingBalances.HandleAsync(new GetOutstandingBalancesQuery(), cancellationToken);

        return Ok(balances);
    }

    /// <summary>
    /// Records a cash or card payment against an invoice. Card payments are recorded with a simulated status.
    /// </summary>
    [HttpPost("{invoiceId:guid}/payments")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RecordPayment(
        Guid invoiceId,
        [FromBody] RecordPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var paymentId = await _recordPayment.HandleAsync(
            new RecordPaymentCommand(invoiceId, request.Amount, request.Method),
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created, paymentId);
    }
}
