using FluentAssertions;
using RestaurantService.Domain.Entities;
using RestaurantService.Domain.Enums;
using RestaurantService.Domain.Events;
using RestaurantService.Domain.Exceptions;

namespace RestaurantService.Tests;

public class RestaurantAggregateTests
{
    private static Restaurant CreateRestaurant() =>
        Restaurant.Create(Guid.NewGuid(), "Test", "Desc", "Addr");

    [Fact]
    public void Create_WithValidData_SetsIsActiveTrue()
    {
        var restaurant = CreateRestaurant();
        restaurant.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithValidData_SetsStatusOpen()
    {
        var restaurant = CreateRestaurant();
        restaurant.Status.Should().Be(RestaurantStatus.Open);
    }

    [Fact]
    public void Create_RaisesRestaurantCreatedDomainEvent()
    {
        var restaurant = CreateRestaurant();
        restaurant.DomainEvents.Should().ContainSingle(e => e is RestaurantCreatedDomainEvent);
    }

    [Fact]
    public void Update_ChangesNameAndDescription()
    {
        var restaurant = CreateRestaurant();
        restaurant.Update("New Name", "New Desc", "New Addr");
        restaurant.Name.Should().Be("New Name");
        restaurant.Description.Should().Be("New Desc");
    }

    [Fact]
    public void Update_RaisesRestaurantUpdatedDomainEvent()
    {
        var restaurant = CreateRestaurant();
        restaurant.ClearDomainEvents();
        restaurant.Update("N", "D", "A");
        restaurant.DomainEvents.Should().ContainSingle(e => e is RestaurantUpdatedDomainEvent);
    }

    [Fact]
    public void AddMenuItem_AddsItemToCollection()
    {
        var restaurant = CreateRestaurant();
        restaurant.AddMenuItem("Burger", "Tasty", 9.99m);
        restaurant.MenuItems.Should().HaveCount(1);
    }

    [Fact]
    public void AddMenuItem_RaisesMenuItemAddedDomainEvent()
    {
        var restaurant = CreateRestaurant();
        restaurant.ClearDomainEvents();
        restaurant.AddMenuItem("Burger", "Tasty", 9.99m);
        restaurant.DomainEvents.Should().ContainSingle(e => e is MenuItemAddedDomainEvent);
    }

    [Fact]
    public void RemoveMenuItem_RemovesFromCollection()
    {
        var restaurant = CreateRestaurant();
        var item = restaurant.AddMenuItem("Burger", "Tasty", 9.99m);
        restaurant.RemoveMenuItem(item.Id);
        restaurant.MenuItems.Should().BeEmpty();
    }

    [Fact]
    public void RemoveMenuItem_WithUnknownId_ThrowsMenuItemNotFoundException()
    {
        var restaurant = CreateRestaurant();
        var act = () => restaurant.RemoveMenuItem(Guid.NewGuid());
        act.Should().Throw<MenuItemNotFoundException>();
    }

    [Fact]
    public void RemoveMenuItem_RaisesMenuItemRemovedDomainEvent()
    {
        var restaurant = CreateRestaurant();
        var item = restaurant.AddMenuItem("Burger", "Tasty", 9.99m);
        restaurant.ClearDomainEvents();
        restaurant.RemoveMenuItem(item.Id);
        restaurant.DomainEvents.Should().ContainSingle(e => e is MenuItemRemovedDomainEvent);
    }

    [Fact]
    public void ChangeAvailability_SetsIsActiveFalse_SetsStatusClosed()
    {
        var restaurant = CreateRestaurant();
        restaurant.ChangeAvailability(false);
        restaurant.IsActive.Should().BeFalse();
        restaurant.Status.Should().Be(RestaurantStatus.Closed);
    }

    [Fact]
    public void ChangeAvailability_RaisesRestaurantAvailabilityChangedDomainEvent()
    {
        var restaurant = CreateRestaurant();
        restaurant.ClearDomainEvents();
        restaurant.ChangeAvailability(false);
        restaurant.DomainEvents.Should().ContainSingle(e => e is RestaurantAvailabilityChangedDomainEvent);
    }
}
