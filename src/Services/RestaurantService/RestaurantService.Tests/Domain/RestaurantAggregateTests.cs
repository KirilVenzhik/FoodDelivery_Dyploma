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
    public void Create_SetsIsActiveTrueAndStatusOpen()
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
    public void Update_ChangesFieldsAndRaisesEvent()
    {
        var r = MakeRestaurant();
        r.ClearDomainEvents();

        r.Update("New Name", "New desc", "New addr");

        r.Name.Should().Be("New Name");
        r.Description.Should().Be("New desc");
        r.DomainEvents.Should().ContainSingle(e => e is RestaurantUpdatedDomainEvent);
    }

    [Fact]
    public void AddMenuItem_AddsToCollectionAndRaisesEvent()
    {
        var r = MakeRestaurant();
        r.ClearDomainEvents();

        r.AddMenuItem("Burger", "Juicy", 8.99m);

        r.MenuItems.Should().HaveCount(1);
        r.DomainEvents.Should().ContainSingle(e => e is MenuItemAddedDomainEvent);
    }

    [Fact]
    public void RemoveMenuItem_RemovesFromCollectionAndRaisesEvent()
    {
        var r = MakeRestaurant();
        var item = r.AddMenuItem("Fries", "Crispy", 3.99m);
        r.ClearDomainEvents();

        r.RemoveMenuItem(item.Id);

        r.MenuItems.Should().BeEmpty();
        r.DomainEvents.Should().ContainSingle(e => e is MenuItemRemovedDomainEvent);
    }

    [Fact]
    public void RemoveMenuItem_UnknownId_ThrowsMenuItemNotFoundException()
    {
        var r = MakeRestaurant();

        var act = () => r.RemoveMenuItem(Guid.NewGuid());

        act.Should().Throw<MenuItemNotFoundException>();
    }

    [Fact]
    public void ChangeAvailability_False_SetsIsActiveFalseAndClosed()
    {
        var r = MakeRestaurant();
        r.ClearDomainEvents();

        r.ChangeAvailability(false);

        r.IsActive.Should().BeFalse();
        r.Status.Should().Be(RestaurantStatus.Closed);
        r.DomainEvents.Should().ContainSingle(e => e is RestaurantAvailabilityChangedDomainEvent);
    }
}
