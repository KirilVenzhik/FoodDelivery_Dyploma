using Xunit;
using FluentAssertions;
using OrderService.Domain.Entities;

namespace OrderService.Tests.Domain;

public class OrderItemTests
{
    [Fact]
    public void Create_SetsAllFields()
    {
        var menuItemId = Guid.NewGuid();
        var item = OrderItem.Create(menuItemId, "Burger", 12.50m, 3);

        item.MenuItemId.Should().Be(menuItemId);
        item.ItemName.Should().Be("Burger");
        item.UnitPrice.Should().Be(12.50m);
        item.Quantity.Should().Be(3);
    }

    [Fact]
    public void Create_CalculatesLineTotal()
    {
        var item = OrderItem.Create(Guid.NewGuid(), "Pizza", 15m, 4);
        item.LineTotal.Should().Be(60m);
    }

    [Fact]
    public void Create_SetsUniqueId()
    {
        var item1 = OrderItem.Create(Guid.NewGuid(), "A", 5m, 1);
        var item2 = OrderItem.Create(Guid.NewGuid(), "B", 5m, 1);
        item1.Id.Should().NotBe(item2.Id);
    }
}
