using RestaurantService.Domain.Enums;
using Shared.Primitives;

namespace RestaurantService.Domain.Entities;

public class Restaurant : Entity
{
    public Guid Id { get; private set; }

    public Guid OwnerUserId { get; private set; }

    public string Name { get; private set; } = default!;

    public RestaurantStatus Status { get; private set; }

    private readonly List<MenuItem> _menuItems = [];

    public IReadOnlyCollection<MenuItem> MenuItems => _menuItems;

    private Restaurant() { }

    public static Restaurant Create(
        Guid ownerUserId,
        string name)
    {
        return new Restaurant
        {
            Id = Guid.NewGuid(),
            OwnerUserId = ownerUserId,
            Name = name,
            Status = RestaurantStatus.Closed
        };
    }

    public void Open()
    {
        Status = RestaurantStatus.Open;
    }

    public void Close()
    {
        Status = RestaurantStatus.Closed;
    }

    public MenuItem AddMenuItem(
        string name,
        decimal price)
    {
        var menuItem = MenuItem.Create(
            name,
            price);

        _menuItems.Add(menuItem);

        return menuItem;
    }

    public bool RemoveMenuItem(Guid menuItemId)
    {
        var item = _menuItems.FirstOrDefault(m => m.Id == menuItemId);
        if (item is null) return false;
        _menuItems.Remove(item);
        return true;
    }
}