using Auth.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.Queries;

namespace NotificationService.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly GetNotificationsByUserQueryHandler _getByUserHandler;
    private readonly ICurrentUser _currentUser;

    public NotificationsController(
        GetNotificationsByUserQueryHandler getByUserHandler,
        ICurrentUser currentUser)
    {
        _getByUserHandler = getByUserHandler;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyNotifications(CancellationToken cancellationToken)
    {
        var notifications = await _getByUserHandler.HandleAsync(
            new GetNotificationsByUserQuery(_currentUser.UserId), cancellationToken);

        return Ok(notifications);
    }
}
