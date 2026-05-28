using Auth.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.Common;
using PaymentService.Application.Queries;

namespace PaymentService.API.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly GetPaymentByOrderIdQueryHandler _getByOrderIdHandler;

    public PaymentsController(GetPaymentByOrderIdQueryHandler getByOrderIdHandler)
        => _getByOrderIdHandler = getByOrderIdHandler;

    [HttpGet("{orderId:guid}")]
    public async Task<IActionResult> GetByOrderId(Guid orderId, CancellationToken cancellationToken)
    {
        var result = await _getByOrderIdHandler.HandleAsync(
            new GetPaymentByOrderIdQuery(orderId), cancellationToken);

        return result.Status switch
        {
            CommandResultStatus.Success => Ok(result.Value),
            CommandResultStatus.NotFound => NotFound(),
            _ => StatusCode(500)
        };
    }
}
