using Shared.Primitives;

namespace RestaurantService.Domain.Events;

public sealed class MenuItemAddedDomainEvent : DomainEvent
{
    public Guid RestaurantId { get; }
    public Guid MenuItemId { get; }
    public string Name { get; }
    public decimal Price { get; }

    public MenuItemAddedDomainEvent(Guid restaurantId, Guid menuItemId, string name, decimal price)
    {
        RestaurantId = restaurantId;
        MenuItemId = menuItemId;
        Name = name;
        Price = price;
    }
}
