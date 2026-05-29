using FluentAssertions;
using Moq;
using NotificationService.Application.Commands;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;

namespace NotificationService.Tests.Application;

public class CreateNotificationCommandHandlerTests
{
    private readonly Mock<INotificationRepository> _repo = new();
    private readonly CreateNotificationCommandHandler _handler;

    public CreateNotificationCommandHandlerTests()
        => _handler = new CreateNotificationCommandHandler(_repo.Object);

    [Fact]
    public async Task Handle_OrderPlacedType_CreatesAndPersistsNotification()
    {
        var cmd = new CreateNotificationCommand(Guid.NewGuid(), Guid.NewGuid(), NotificationType.OrderPlaced);

        var result = await _handler.HandleAsync(cmd);

        result.Type.Should().Be("OrderPlaced");
        _repo.Verify(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_PaymentSuccessType_CreatesWithCorrectMessage()
    {
        var orderId = Guid.NewGuid();
        var cmd = new CreateNotificationCommand(Guid.NewGuid(), orderId, NotificationType.PaymentSuccess);

        var result = await _handler.HandleAsync(cmd);

        result.Message.Should().Contain(orderId.ToString());
        result.Type.Should().Be("PaymentSuccess");
    }

    [Fact]
    public async Task Handle_PaymentFailedType_CreatesWithCorrectMessage()
    {
        var cmd = new CreateNotificationCommand(Guid.NewGuid(), Guid.NewGuid(), NotificationType.PaymentFailed);

        var result = await _handler.HandleAsync(cmd);

        result.Type.Should().Be("PaymentFailed");
        result.Message.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Handle_CallsMarkAsSent_SetsSentAt()
    {
        var cmd = new CreateNotificationCommand(Guid.NewGuid(), Guid.NewGuid(), NotificationType.OrderDelivered);

        var result = await _handler.HandleAsync(cmd);

        result.SentAt.Should().NotBeNull();
    }
}
