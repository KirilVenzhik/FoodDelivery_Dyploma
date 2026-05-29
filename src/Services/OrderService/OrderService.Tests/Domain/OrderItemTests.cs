using FluentAssertions;
using OrderService.Domain.Entities;
using Xunit;

namespace OrderService.Tests.Domain;

public class OrderItemTests
{
    [Fact]
    public void Create_CalculatesLineTotalCorrectly()
    {
        var item = OrderItem.Create(Guid.NewGuid(), "Burger", 8.50m, 3);

        item.LineTotal.Should().Be(25.50m);
        item.UnitPrice.Should().Be(8.50m);
        item.Quantity.Should().Be(3);
    }

    [Fact]
    public void UpdateQuantity_RecalculatesLineTotal()
    {
        var item = OrderItem.Create(Guid.NewGuid(), "Burger", 8.50m, 3);

        item.UpdateQuantity(5);

        item.Quantity.Should().Be(5);
        item.LineTotal.Should().Be(42.50m);
    }
}
