using Contracts.Events;
using EventBus.Abstractions;
using FluentAssertions;
using Moq;
using PaymentService.Application.Commands;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Exceptions;

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
    public async Task Handle_NewPayment_CreatesAndCompletesPayment()
    {
        _repo.Setup(r => r.GetByOrderIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        var command = new ProcessPaymentCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 50m);

        var result = await _handler.HandleAsync(command);

        result.Status.Should().Be("Completed");
        result.Amount.Should().Be(50m);
        _repo.Verify(r => r.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NewPayment_PublishesPaymentCompletedEvent()
    {
        _repo.Setup(r => r.GetByOrderIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        var orderId = Guid.NewGuid();
        var command = new ProcessPaymentCommand(orderId, Guid.NewGuid(), Guid.NewGuid(), 50m);

        await _handler.HandleAsync(command);

        _eventBus.Verify(e => e.PublishAsync(
            It.Is<PaymentCompletedEvent>(evt => evt.OrderId == orderId),
            "payment.completed",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateOrderId_ThrowsDuplicatePaymentException()
    {
        var existing = Payment.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 50m);
        _repo.Setup(r => r.GetByOrderIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var act = () => _handler.HandleAsync(
            new ProcessPaymentCommand(existing.OrderId, Guid.NewGuid(), Guid.NewGuid(), 50m));

        await act.Should().ThrowAsync<DuplicatePaymentException>();
    }
}
