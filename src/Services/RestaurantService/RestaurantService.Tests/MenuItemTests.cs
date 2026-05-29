using FluentAssertions;
using RestaurantService.Domain.Entities;
using RestaurantService.Domain.Exceptions;

namespace RestaurantService.Tests;

public class MenuItemTests
{
    [Fact]
    public void Create_WithValidPrice_Succeeds()
    {
        var item = MenuItem.Create(Guid.NewGuid(), "Burger", "Desc", 9.99m);
        item.Price.Should().Be(9.99m);
        item.Available.Should().BeTrue();
    }

    [Fact]
    public void UpdatePrice_WithValidPrice_ChangesPrice()
    {
        var item = MenuItem.Create(Guid.NewGuid(), "Burger", "Desc", 9.99m);
        item.UpdatePrice(15.00m);
        item.Price.Should().Be(15.00m);
    }

    [Fact]
    public void UpdatePrice_WithZeroPrice_ThrowsInvalidMenuItemPriceException()
    {
        var item = MenuItem.Create(Guid.NewGuid(), "Burger", "Desc", 9.99m);
        var act = () => item.UpdatePrice(0m);
        act.Should().Throw<InvalidMenuItemPriceException>();
    }

    [Fact]
    public void UpdatePrice_WithNegativePrice_ThrowsInvalidMenuItemPriceException()
    {
        var item = MenuItem.Create(Guid.NewGuid(), "Burger", "Desc", 9.99m);
        var act = () => item.UpdatePrice(-1m);
        act.Should().Throw<InvalidMenuItemPriceException>();
    }

    [Fact]
    public void SetAvailability_ChangesAvailableFlag()
    {
        var item = MenuItem.Create(Guid.NewGuid(), "Burger", "Desc", 9.99m);
        item.SetAvailability(false);
        item.Available.Should().BeFalse();
    }
}
