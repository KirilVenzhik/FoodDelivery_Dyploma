using FluentAssertions;
using RestaurantService.Domain.Entities;
using RestaurantService.Domain.Exceptions;

namespace RestaurantService.Tests.Domain;

public class MenuItemTests
{
    [Fact]
    public void Create_WithValidPrice_SetsAvailableTrue()
    {
        var item = MenuItem.Create(Guid.NewGuid(), "Burger", "Juicy", 9.99m);

        item.Price.Should().Be(9.99m);
        item.Available.Should().BeTrue();
    }

    [Fact]
    public void UpdatePrice_ValidPrice_UpdatesPrice()
    {
        var item = MenuItem.Create(Guid.NewGuid(), "Burger", "Juicy", 9.99m);

        item.UpdatePrice(12.00m);

        item.Price.Should().Be(12.00m);
    }

    [Fact]
    public void UpdatePrice_ZeroPrice_ThrowsInvalidMenuItemPriceException()
    {
        var item = MenuItem.Create(Guid.NewGuid(), "Burger", "Juicy", 9.99m);

        var act = () => item.UpdatePrice(0m);

        act.Should().Throw<InvalidMenuItemPriceException>();
    }

    [Fact]
    public void UpdatePrice_NegativePrice_ThrowsInvalidMenuItemPriceException()
    {
        var item = MenuItem.Create(Guid.NewGuid(), "Burger", "Juicy", 9.99m);

        var act = () => item.UpdatePrice(-5m);

        act.Should().Throw<InvalidMenuItemPriceException>();
    }

    [Fact]
    public void SetAvailability_False_SetsAvailableFalse()
    {
        var item = MenuItem.Create(Guid.NewGuid(), "Burger", "Juicy", 9.99m);

        item.SetAvailability(false);

        item.Available.Should().BeFalse();
    }
}
