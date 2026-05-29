using FluentAssertions;
using Moq;
using NotificationService.Application.Commands;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Domain.Exceptions;

namespace NotificationService.Tests.Application;

public class MarkNotificationReadCommandHandlerTests
{
    private readonly Mock<INotificationRepository> _repo = new();
    private readonly MarkNotificationReadCommandHandler _handler;

    public MarkNotificationReadCommandHandlerTests()
        => _handler = new MarkNotificationReadCommandHandler(_repo.Object);

    private static Notification MakeNotification(Guid userId)
        => Notification.Create(userId, Guid.NewGuid(), "Test", NotificationType.OrderPlaced);

    [Fact]
    public async Task Handle_OwnNotification_MarksAsRead()
    {
        var userId = Guid.NewGuid();
        var notification = MakeNotification(userId);
        _repo.Setup(r => r.GetByIdAsync(notification.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);

        var result = await _handler.HandleAsync(new MarkNotificationReadCommand(notification.Id, userId));

        result.IsRead.Should().BeTrue();
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UnknownId_ThrowsNotificationNotFoundException()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Notification?)null);

        var act = () => _handler.HandleAsync(new MarkNotificationReadCommand(id, Guid.NewGuid()));

        await act.Should().ThrowAsync<NotificationNotFoundException>();
    }

    [Fact]
    public async Task Handle_OtherUsersNotification_ThrowsUnauthorizedAccessException()
    {
        var notification = MakeNotification(Guid.NewGuid());
        _repo.Setup(r => r.GetByIdAsync(notification.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);

        var act = () => _handler.HandleAsync(new MarkNotificationReadCommand(notification.Id, Guid.NewGuid()));

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
