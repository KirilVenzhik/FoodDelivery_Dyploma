using RestaurantService.Domain.Enums;
using RestaurantService.Domain.Events;
using RestaurantService.Domain.Exceptions;
using Shared.Primitives;

namespace RestaurantService.Domain.Entities;

public class Restaurant : Entity
{
    public Guid Id { get; private set; }
    public Guid OwnerId { get; private set; }
    public string Name { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public string Address { get; private set; } = default!;
    public bool IsActive { get; private set; }
    public RestaurantStatus Status { get; private set; }

    private readonly List<MenuItem> _menuItems = [];
    public IReadOnlyCollection<MenuItem> MenuItems => _menuItems;

    private Restaurant() { }

    public static Restaurant Create(
        Guid ownerId,
        string name,
        string description,
        string address)
    {
        var restaurant = new Restaurant
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId,
            Name = name,
            Description = description,
            Address = address,
            IsActive = true,
            Status = RestaurantStatus.Open
        };

        restaurant.RaiseDomainEvent(
            new RestaurantCreatedDomainEvent(restaurant.Id, ownerId, name));

        return restaurant;
    }

    public void Update(string name, string description, string address)
    {
        Name = name;
        Description = description;
        Address = address;
        RaiseDomainEvent(new RestaurantUpdatedDomainEvent(Id, name));
    }

    public MenuItem AddMenuItem(string name, string description, decimal price)
    {
        var menuItem = MenuItem.Create(Id, name, description, price);
        _menuItems.Add(menuItem);
        RaiseDomainEvent(new MenuItemAddedDomainEvent(Id, menuItem.Id, name, price));
        return menuItem;
    }

    public void RemoveMenuItem(Guid menuItemId)
    {
        var item = _menuItems.FirstOrDefault(m => m.Id == menuItemId)
            ?? throw new MenuItemNotFoundException(menuItemId);
        _menuItems.Remove(item);
        RaiseDomainEvent(new MenuItemRemovedDomainEvent(Id, menuItemId));
    }

    public void ChangeAvailability(bool isActive)
    {
        IsActive = isActive;
        Status = isActive ? RestaurantStatus.Open : RestaurantStatus.Closed;
        RaiseDomainEvent(new RestaurantAvailabilityChangedDomainEvent(Id, isActive));
    }

    public void UpdateMenuItem(
        Guid menuItemId,
        string? name,
        string? description,
        decimal? price,
        bool? available)
    {
        var item = _menuItems.FirstOrDefault(m => m.Id == menuItemId)
            ?? throw new MenuItemNotFoundException(menuItemId);

        if (name is not null) item.UpdateName(name);
        if (description is not null) item.UpdateDescription(description);
        if (price.HasValue) item.UpdatePrice(price.Value);
        if (available.HasValue) item.SetAvailability(available.Value);

        RaiseDomainEvent(new MenuItemUpdatedDomainEvent(Id, menuItemId));
    }
}
