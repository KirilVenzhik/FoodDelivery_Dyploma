using Shared.Primitives;

namespace RestaurantService.Domain.Events;

public sealed class MenuItemUpdatedDomainEvent : DomainEvent
{
    public Guid RestaurantId { get; }
    public Guid MenuItemId { get; }

    public MenuItemUpdatedDomainEvent(Guid restaurantId, Guid menuItemId)
    {
        RestaurantId = restaurantId;
        MenuItemId = menuItemId;
    }
}
