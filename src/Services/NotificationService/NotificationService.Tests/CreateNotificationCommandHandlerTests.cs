using FluentAssertions;
using Moq;
using NotificationService.Application.Commands;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;

namespace NotificationService.Tests;

public class CreateNotificationCommandHandlerTests
{
    private readonly Mock<INotificationRepository> _mockRepo = new();
    private readonly CreateNotificationCommandHandler _handler;

    public CreateNotificationCommandHandlerTests()
    {
        _mockRepo.Setup(r => r.AddAsync(It.IsAny<Notification>(), default)).Returns(Task.CompletedTask);
        _mockRepo.Setup(r => r.SaveChangesAsync(default)).Returns(Task.CompletedTask);
        _handler = new CreateNotificationCommandHandler(_mockRepo.Object);
    }

    [Fact]
    public async Task Handle_OrderPlacedType_CreatesAndPersistsNotification()
    {
        var command = new CreateNotificationCommand(Guid.NewGuid(), Guid.NewGuid(), NotificationType.OrderPlaced);

        var result = await _handler.HandleAsync(command);

        result.Should().NotBeNull();
        result.UserId.Should().Be(command.UserId);
        result.OrderId.Should().Be(command.OrderId);
        result.Type.Should().Be(NotificationType.OrderPlaced.ToString());
        _mockRepo.Verify(r => r.AddAsync(It.IsAny<Notification>(), default), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_PaymentSuccessType_CreatesWithCorrectMessage()
    {
        var orderId = Guid.NewGuid();
        var command = new CreateNotificationCommand(Guid.NewGuid(), orderId, NotificationType.PaymentSuccess);

        var result = await _handler.HandleAsync(command);

        result.Message.Should().Contain(orderId.ToString());
    }

    [Fact]
    public async Task Handle_PaymentFailedType_CreatesWithCorrectMessage()
    {
        var orderId = Guid.NewGuid();
        var command = new CreateNotificationCommand(Guid.NewGuid(), orderId, NotificationType.PaymentFailed);

        var result = await _handler.HandleAsync(command);

        result.Message.Should().Contain(orderId.ToString());
    }

    [Fact]
    public async Task Handle_CallsMarkAsSent()
    {
        var command = new CreateNotificationCommand(Guid.NewGuid(), Guid.NewGuid(), NotificationType.OrderPlaced);

        var result = await _handler.HandleAsync(command);

        result.SentAt.Should().NotBeNull();
    }
}
