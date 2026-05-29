using FluentAssertions;
using Moq;
using NotificationService.Application.Commands;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Domain.Exceptions;

namespace NotificationService.Tests;

public class MarkNotificationReadCommandHandlerTests
{
    private readonly Mock<INotificationRepository> _mockRepo = new();
    private readonly MarkNotificationReadCommandHandler _handler;

    public MarkNotificationReadCommandHandlerTests()
    {
        _mockRepo.Setup(r => r.SaveChangesAsync(default)).Returns(Task.CompletedTask);
        _handler = new MarkNotificationReadCommandHandler(_mockRepo.Object);
    }

    [Fact]
    public async Task Handle_OwnNotification_MarksAsRead()
    {
        var userId = Guid.NewGuid();
        var notification = Notification.Create(userId, Guid.NewGuid(), "msg", NotificationType.OrderPlaced);
        _mockRepo.Setup(r => r.GetByIdAsync(notification.Id, default)).ReturnsAsync(notification);

        var result = await _handler.HandleAsync(new MarkNotificationReadCommand(notification.Id, userId));

        result.IsRead.Should().BeTrue();
        _mockRepo.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_UnknownId_ThrowsNotificationNotFoundException()
    {
        var id = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetByIdAsync(id, default)).ReturnsAsync((Notification?)null);

        var act = async () => await _handler.HandleAsync(new MarkNotificationReadCommand(id, Guid.NewGuid()));

        await act.Should().ThrowAsync<NotificationNotFoundException>();
    }

    [Fact]
    public async Task Handle_OtherUsersNotification_ThrowsUnauthorizedAccessException()
    {
        var ownerId = Guid.NewGuid();
        var notification = Notification.Create(ownerId, Guid.NewGuid(), "msg", NotificationType.PaymentSuccess);
        _mockRepo.Setup(r => r.GetByIdAsync(notification.Id, default)).ReturnsAsync(notification);

        var act = async () => await _handler.HandleAsync(
            new MarkNotificationReadCommand(notification.Id, Guid.NewGuid()));

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
