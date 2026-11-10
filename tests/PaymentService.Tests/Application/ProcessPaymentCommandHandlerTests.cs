using Contracts.Events;
using EventBus.Abstractions;
using FluentAssertions;
using Moq;
using PaymentService.Application.Commands;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;

namespace PaymentService.Tests.Application;

public class ProcessPaymentCommandHandlerTests
{
    private readonly Mock<IPaymentRepository> _repo = new();
    private readonly Mock<IEventBus> _eventBus = new();
    private readonly ProcessPaymentCommandHandler _handler;

    public ProcessPaymentCommandHandlerTests()
    {
        _handler = new ProcessPaymentCommandHandler(_repo.Object, _eventBus.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_PublishesPaymentCompletedWithClientAndRestaurantIds()
    {
        var command = new ProcessPaymentCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 25m);

        PaymentCompletedEvent? published = null;
        _eventBus
            .Setup(b => b.PublishAsync(It.IsAny<PaymentCompletedEvent>(), "payment.completed", It.IsAny<CancellationToken>()))
            .Callback<PaymentCompletedEvent, string, CancellationToken>((e, _, _) => published = e)
            .Returns(Task.CompletedTask);

        await _handler.HandleAsync(command);

        published.Should().NotBeNull();
        published!.OrderId.Should().Be(command.OrderId);
        published.ClientId.Should().Be(command.ClientId);
        published.RestaurantId.Should().Be(command.RestaurantId);
        published.Amount.Should().Be(25m);
    }

    [Fact]
    public async Task Handle_PaymentAlreadyExists_Throws()
    {
        var command = new ProcessPaymentCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 25m);
        _repo.Setup(r => r.GetByOrderIdAsync(command.OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Payment.Create(command.OrderId, command.CorrelationId, command.ClientId, 25m));

        var act = () => _handler.HandleAsync(command);

        await act.Should().ThrowAsync<PaymentService.Domain.Exceptions.DuplicatePaymentException>();
        _eventBus.Verify(b => b.PublishAsync(It.IsAny<PaymentCompletedEvent>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
