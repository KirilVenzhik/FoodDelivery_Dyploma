using FluentAssertions;
using Moq;
using NotificationService.Application.Interfaces;
using NotificationService.Application.Queries;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;

namespace NotificationService.Tests;

public class GetMyNotificationsQueryHandlerTests
{
    private readonly Mock<INotificationRepository> _mockRepo = new();
    private readonly GetMyNotificationsQueryHandler _handler;

    public GetMyNotificationsQueryHandlerTests()
    {
        _handler = new GetMyNotificationsQueryHandler(_mockRepo.Object);
    }

    [Fact]
    public async Task Handle_ReturnsUserNotificationsOnly()
    {
        var userId = Guid.NewGuid();
        var n1 = Notification.Create(userId, Guid.NewGuid(), "msg1", NotificationType.OrderPlaced);
        var n2 = Notification.Create(userId, Guid.NewGuid(), "msg2", NotificationType.PaymentSuccess);

        _mockRepo
            .Setup(r => r.GetByUserIdAsync(userId, default))
            .ReturnsAsync([n1, n2]);

        var result = await _handler.HandleAsync(new GetMyNotificationsQuery(userId));

        result.Should().HaveCount(2);
        result.Should().OnlyContain(r => r.UserId == userId);
    }

    [Fact]
    public async Task Handle_ReturnsOrderedByCreatedAtDescending()
    {
        var userId = Guid.NewGuid();
        var older = Notification.Create(userId, Guid.NewGuid(), "older", NotificationType.OrderPlaced);
        var newer = Notification.Create(userId, Guid.NewGuid(), "newer", NotificationType.PaymentSuccess);

        _mockRepo
            .Setup(r => r.GetByUserIdAsync(userId, default))
            .ReturnsAsync([newer, older]);

        var result = await _handler.HandleAsync(new GetMyNotificationsQuery(userId));

        result[0].Message.Should().Be("newer");
        result[1].Message.Should().Be("older");
    }
}
