using FluentAssertions;
using RestaurantService.Domain.Entities;
using RestaurantService.Domain.Exceptions;

namespace RestaurantService.Tests.Domain;

public class MenuItemTests
{
    [Fact]
    public void Create_WithValidPrice_Succeeds()
    {
        var item = MenuItem.Create(Guid.NewGuid(), "Burger", "Juicy", 9.99m);

        item.Price.Should().Be(9.99m);
        item.Available.Should().BeTrue();
    }

    [Fact]
    public void UpdatePrice_WithZeroPrice_ThrowsInvalidMenuItemPriceException()
    {
        var item = MenuItem.Create(Guid.NewGuid(), "Burger", "Juicy", 9.99m);

        var act = () => item.UpdatePrice(0m);

        act.Should().Throw<InvalidMenuItemPriceException>();
    }

    [Fact]
    public void UpdatePrice_WithNegativePrice_ThrowsInvalidMenuItemPriceException()
    {
        var item = MenuItem.Create(Guid.NewGuid(), "Burger", "Juicy", 9.99m);

        var act = () => item.UpdatePrice(-5m);

        act.Should().Throw<InvalidMenuItemPriceException>();
    }
}
