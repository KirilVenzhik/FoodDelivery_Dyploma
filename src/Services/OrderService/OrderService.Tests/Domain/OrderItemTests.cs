using FluentAssertions;
using OrderService.Domain.Entities;

namespace OrderService.Tests.Domain;

public class OrderItemTests
{
    [Fact]
    public void Create_ComputesLineTotal()
    {
        var item = OrderItem.Create(Guid.NewGuid(), "Pizza", 12.50m, 3);

        item.UnitPrice.Should().Be(12.50m);
        item.Quantity.Should().Be(3);
        item.LineTotal.Should().Be(37.50m);
    }

    [Fact]
    public void UpdateQuantity_RecalculatesLineTotal()
    {
        var item = OrderItem.Create(Guid.NewGuid(), "Pizza", 10m, 1);

        item.UpdateQuantity(4);

        item.Quantity.Should().Be(4);
        item.LineTotal.Should().Be(40m);
    }
}
