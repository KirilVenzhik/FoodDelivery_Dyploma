using FluentAssertions;
using Moq;
using PaymentService.Application.Interfaces;
using PaymentService.Application.Queries;
using PaymentService.Domain.Entities;

namespace PaymentService.Tests.Application;

public class GetPaymentByOrderIdQueryHandlerTests
{
    private readonly Mock<IPaymentRepository> _repo = new();
    private readonly GetPaymentByOrderIdQueryHandler _handler;

    public GetPaymentByOrderIdQueryHandlerTests()
    {
        _handler = new GetPaymentByOrderIdQueryHandler(_repo.Object);
    }

    private Payment SetupPayment(Guid clientId)
    {
        var payment = Payment.Create(Guid.NewGuid(), Guid.NewGuid(), clientId, 25m);
        _repo.Setup(r => r.GetByOrderIdAsync(payment.OrderId, It.IsAny<CancellationToken>())).ReturnsAsync(payment);
        return payment;
    }

    [Fact]
    public async Task Handle_PayingClient_ReturnsPayment()
    {
        var clientId = Guid.NewGuid();
        var payment = SetupPayment(clientId);

        var result = await _handler.HandleAsync(new GetPaymentByOrderIdQuery(payment.OrderId, clientId));

        result.OrderId.Should().Be(payment.OrderId);
    }

    [Fact]
    public async Task Handle_AnotherUser_ThrowsUnauthorizedAccessException()
    {
        var payment = SetupPayment(Guid.NewGuid());

        var act = () => _handler.HandleAsync(new GetPaymentByOrderIdQuery(payment.OrderId, Guid.NewGuid()));

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
