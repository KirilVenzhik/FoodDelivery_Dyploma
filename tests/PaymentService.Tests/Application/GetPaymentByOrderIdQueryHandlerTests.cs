using Xunit;
using FluentAssertions;
using Moq;
using PaymentService.Application.Common;
using PaymentService.Application.Interfaces;
using PaymentService.Application.Queries;
using PaymentService.Domain.Entities;

namespace PaymentService.Tests.Application;

public class GetPaymentByOrderIdQueryHandlerTests
{
    private readonly Mock<IPaymentRepository> _repositoryMock = new();
    private readonly GetPaymentByOrderIdQueryHandler _sut;

    public GetPaymentByOrderIdQueryHandlerTests()
    {
        _sut = new GetPaymentByOrderIdQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenPaymentNotFound_ReturnsNotFound()
    {
        _repositoryMock.Setup(x => x.GetByOrderIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        var result = await _sut.HandleAsync(new GetPaymentByOrderIdQuery(Guid.NewGuid()));

        result.Status.Should().Be(CommandResultStatus.NotFound);
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenPaymentFound_ReturnsSuccess()
    {
        var orderId = Guid.NewGuid();
        var payment = Payment.Create(orderId, Guid.NewGuid(), 50m);
        _repositoryMock.Setup(x => x.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var result = await _sut.HandleAsync(new GetPaymentByOrderIdQuery(orderId));

        result.Status.Should().Be(CommandResultStatus.Success);
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_MapsAllFieldsCorrectly()
    {
        var orderId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();
        var payment = Payment.Create(orderId, correlationId, 123.45m);
        payment.Complete();
        _repositoryMock.Setup(x => x.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var result = await _sut.HandleAsync(new GetPaymentByOrderIdQuery(orderId));

        result.Value!.OrderId.Should().Be(orderId);
        result.Value.CorrelationId.Should().Be(correlationId);
        result.Value.Amount.Should().Be(123.45m);
        result.Value.Status.Should().Be("Completed");
        result.Value.ProcessedAt.Should().NotBeNull();
    }
}
