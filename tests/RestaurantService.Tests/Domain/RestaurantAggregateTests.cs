using FluentAssertions;
using RestaurantService.Domain.Entities;
using RestaurantService.Domain.Enums;
using RestaurantService.Domain.Events;
using RestaurantService.Domain.Exceptions;

namespace RestaurantService.Tests.Domain;

public class RestaurantAggregateTests
{
    private static Restaurant MakeRestaurant()
        => Restaurant.Create(Guid.NewGuid(), "Bistro", "Good food", "123 Main St");

    [Fact]
    public void Create_WithValidData_SetsIsActiveTrue()
    {
        var r = MakeRestaurant();

        r.IsActive.Should().BeTrue();
        r.Status.Should().Be(RestaurantStatus.Open);
    }

    [Fact]
    public void Create_RaisesRestaurantCreatedDomainEvent()
    {
        var r = MakeRestaurant();

        r.DomainEvents.Should().ContainSingle(e => e is RestaurantCreatedDomainEvent);
    }

    [Fact]
    public void AddMenuItem_AddsItemToCollection()
    {
        var r = MakeRestaurant();

        r.AddMenuItem("Burger", "Juicy", 8.99m);

        r.MenuItems.Should().HaveCount(1);
    }

    [Fact]
    public void AddMenuItem_RaisesMenuItemAddedDomainEvent()
    {
        var r = MakeRestaurant();
        r.ClearDomainEvents();

        r.AddMenuItem("Burger", "Juicy", 8.99m);

        r.DomainEvents.Should().ContainSingle(e => e is MenuItemAddedDomainEvent);
    }

    [Fact]
    public void RemoveMenuItem_RemovesFromCollection()
    {
        var r = MakeRestaurant();
        var item = r.AddMenuItem("Fries", "Crispy", 3.99m);
        r.ClearDomainEvents();

        r.RemoveMenuItem(item.Id);

        r.MenuItems.Should().BeEmpty();
    }

    [Fact]
    public void RemoveMenuItem_WithUnknownId_ThrowsMenuItemNotFoundException()
    {
        var r = MakeRestaurant();

        var act = () => r.RemoveMenuItem(Guid.NewGuid());

        act.Should().Throw<MenuItemNotFoundException>();
    }

    [Fact]
    public void ChangeAvailability_SetsIsActiveFalse()
    {
        var r = MakeRestaurant();
        r.ClearDomainEvents();

        r.ChangeAvailability(false);

        r.IsActive.Should().BeFalse();
        r.Status.Should().Be(RestaurantStatus.Closed);
    }
}
