using Auth.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.Commands;
using NotificationService.Application.Queries;

namespace NotificationService.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly GetMyNotificationsQueryHandler _getMyHandler;
    private readonly GetNotificationByIdQueryHandler _getByIdHandler;
    private readonly MarkNotificationReadCommandHandler _markReadHandler;
    private readonly ICurrentUser _currentUser;

    public NotificationsController(
        GetMyNotificationsQueryHandler getMyHandler,
        GetNotificationByIdQueryHandler getByIdHandler,
        MarkNotificationReadCommandHandler markReadHandler,
        ICurrentUser currentUser)
    {
        _getMyHandler   = getMyHandler;
        _getByIdHandler = getByIdHandler;
        _markReadHandler = markReadHandler;
        _currentUser    = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyNotifications(CancellationToken cancellationToken)
    {
        var result = await _getMyHandler.HandleAsync(
            new GetMyNotificationsQuery(_currentUser.UserId), cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getByIdHandler.HandleAsync(
            new GetNotificationByIdQuery(id, _currentUser.UserId), cancellationToken);

        return Ok(result);
    }

    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        var result = await _markReadHandler.HandleAsync(
            new MarkNotificationReadCommand(id, _currentUser.UserId), cancellationToken);

        return Ok(result);
    }
}
