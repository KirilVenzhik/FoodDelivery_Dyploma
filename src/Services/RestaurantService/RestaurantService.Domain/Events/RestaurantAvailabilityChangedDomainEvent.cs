using Shared.Primitives;

namespace RestaurantService.Domain.Events;

public sealed class RestaurantAvailabilityChangedDomainEvent : DomainEvent
{
    public Guid RestaurantId { get; }
    public bool IsActive { get; }

    public RestaurantAvailabilityChangedDomainEvent(Guid restaurantId, bool isActive)
    {
        RestaurantId = restaurantId;
        IsActive = isActive;
    }
}
