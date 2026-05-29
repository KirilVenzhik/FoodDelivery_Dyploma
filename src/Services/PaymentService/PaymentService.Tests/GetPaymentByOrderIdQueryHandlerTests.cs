using FluentAssertions;
using Moq;
using PaymentService.Application.Interfaces;
using PaymentService.Application.Queries;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Exceptions;

namespace PaymentService.Tests;

public class GetPaymentByOrderIdQueryHandlerTests
{
    private readonly Mock<IPaymentRepository> _repositoryMock = new();
    private readonly GetPaymentByOrderIdQueryHandler _handler;

    public GetPaymentByOrderIdQueryHandlerTests()
    {
        _handler = new GetPaymentByOrderIdQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingPayment_ReturnsPaymentResponse()
    {
        var clientId = Guid.NewGuid();
        var payment = Payment.Create(Guid.NewGuid(), Guid.NewGuid(), clientId, 75m);

        _repositoryMock
            .Setup(r => r.GetByOrderIdAsync(payment.OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var query = new GetPaymentByOrderIdQuery(payment.OrderId, clientId, "Customer");

        var response = await _handler.HandleAsync(query);

        response.OrderId.Should().Be(payment.OrderId);
        response.ClientId.Should().Be(clientId);
        response.Amount.Should().Be(75m);
    }

    [Fact]
    public async Task Handle_UnknownOrderId_ThrowsPaymentNotFoundException()
    {
        _repositoryMock
            .Setup(r => r.GetByOrderIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        var query = new GetPaymentByOrderIdQuery(
            Guid.NewGuid(), Guid.NewGuid(), "Customer");

        var act = async () => await _handler.HandleAsync(query);

        await act.Should().ThrowAsync<PaymentNotFoundException>();
    }

    [Fact]
    public async Task Handle_CustomerAccessingOthersPayment_ThrowsUnauthorizedAccessException()
    {
        var ownerClientId = Guid.NewGuid();
        var otherClientId = Guid.NewGuid();
        var payment = Payment.Create(Guid.NewGuid(), Guid.NewGuid(), ownerClientId, 75m);

        _repositoryMock
            .Setup(r => r.GetByOrderIdAsync(payment.OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var query = new GetPaymentByOrderIdQuery(payment.OrderId, otherClientId, "Customer");

        var act = async () => await _handler.HandleAsync(query);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
