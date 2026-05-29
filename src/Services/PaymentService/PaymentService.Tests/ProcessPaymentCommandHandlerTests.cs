using Contracts.Events;
using EventBus.Abstractions;
using FluentAssertions;
using Moq;
using PaymentService.Application.Commands;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Exceptions;

namespace PaymentService.Tests;

public class ProcessPaymentCommandHandlerTests
{
    private readonly Mock<IPaymentRepository> _repositoryMock = new();
    private readonly Mock<IEventBus> _eventBusMock = new();
    private readonly ProcessPaymentCommandHandler _handler;

    public ProcessPaymentCommandHandlerTests()
    {
        _handler = new ProcessPaymentCommandHandler(
            _repositoryMock.Object,
            _eventBusMock.Object);
    }

    [Fact]
    public async Task Handle_NewPayment_CreatesAndCompletesPayment()
    {
        _repositoryMock
            .Setup(r => r.GetByOrderIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        var command = new ProcessPaymentCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 50m);

        var response = await _handler.HandleAsync(command);

        response.Status.Should().Be("Completed");
        response.ProcessedAt.Should().NotBeNull();
        response.Amount.Should().Be(50m);
    }

    [Fact]
    public async Task Handle_NewPayment_PublishesPaymentCompletedEvent()
    {
        _repositoryMock
            .Setup(r => r.GetByOrderIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        var command = new ProcessPaymentCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 50m);

        await _handler.HandleAsync(command);

        _eventBusMock.Verify(
            e => e.PublishAsync(
                It.IsAny<PaymentCompletedEvent>(),
                "payment.completed",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateOrderId_ThrowsDuplicatePaymentException()
    {
        var existingPayment = Payment.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 100m);

        _repositoryMock
            .Setup(r => r.GetByOrderIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPayment);

        var command = new ProcessPaymentCommand(
            existingPayment.OrderId, Guid.NewGuid(), Guid.NewGuid(), 100m);

        var act = async () => await _handler.HandleAsync(command);

        await act.Should().ThrowAsync<DuplicatePaymentException>();
    }
}
