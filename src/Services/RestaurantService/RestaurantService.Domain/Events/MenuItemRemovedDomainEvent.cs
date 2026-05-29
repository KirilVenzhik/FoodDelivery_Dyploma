using Shared.Primitives;

namespace RestaurantService.Domain.Events;

public sealed class MenuItemRemovedDomainEvent : DomainEvent
{
    public Guid RestaurantId { get; }
    public Guid MenuItemId { get; }

    public MenuItemRemovedDomainEvent(Guid restaurantId, Guid menuItemId)
    {
        RestaurantId = restaurantId;
        MenuItemId = menuItemId;
    }
}
