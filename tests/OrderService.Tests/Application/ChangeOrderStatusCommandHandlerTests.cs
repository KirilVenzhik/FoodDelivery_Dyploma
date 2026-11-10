using FluentAssertions;
using Moq;
using OrderService.Application.Commands;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;

namespace OrderService.Tests.Application;

public class ChangeOrderStatusCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _repo = new();
    private readonly Mock<IRestaurantServiceClient> _restaurantClient = new();
    private readonly ChangeOrderStatusCommandHandler _handler;

    public ChangeOrderStatusCommandHandlerTests()
    {
        _handler = new ChangeOrderStatusCommandHandler(_repo.Object, _restaurantClient.Object);
    }

    private Order SetupPaidOrder(Guid ownerId)
    {
        var item = OrderItem.Create(Guid.NewGuid(), "Burger", 10m, 1);
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), [item], Guid.NewGuid());
        order.StartPayment();
        order.CompletePayment();

        _repo.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _restaurantClient.Setup(c => c.GetRestaurantWithItemsAsync(order.RestaurantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RestaurantDto { Id = order.RestaurantId, OwnerId = ownerId, Name = "Test Bistro", Status = "Open" });

        return order;
    }

    [Fact]
    public async Task Handle_OwnerOfOrderRestaurant_ChangesStatus()
    {
        var ownerId = Guid.NewGuid();
        var order = SetupPaidOrder(ownerId);

        var result = await _handler.HandleAsync(new ChangeOrderStatusCommand(order.Id, "InPreparation", ownerId));

        result.Status.Should().Be(OrderStatus.InPreparation.ToString());
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_OwnerOfAnotherRestaurant_ThrowsUnauthorizedAccessException()
    {
        var order = SetupPaidOrder(Guid.NewGuid());

        var act = () => _handler.HandleAsync(new ChangeOrderStatusCommand(order.Id, "InPreparation", Guid.NewGuid()));

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        order.Status.Should().Be(OrderStatus.Paid);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
