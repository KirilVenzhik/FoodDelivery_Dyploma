using Auth.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.Queries;

namespace PaymentService.API.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly GetPaymentByOrderIdQueryHandler _queryHandler;
    private readonly ICurrentUser _currentUser;

    public PaymentsController(
        GetPaymentByOrderIdQueryHandler queryHandler,
        ICurrentUser currentUser)
    {
        _queryHandler = queryHandler;
        _currentUser = currentUser;
    }

    [HttpGet("{orderId:guid}")]
    public async Task<IActionResult> GetByOrderId(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        var response = await _queryHandler.HandleAsync(
            new GetPaymentByOrderIdQuery(orderId, _currentUser.UserId),
            cancellationToken);

        return Ok(response);
    }
}
