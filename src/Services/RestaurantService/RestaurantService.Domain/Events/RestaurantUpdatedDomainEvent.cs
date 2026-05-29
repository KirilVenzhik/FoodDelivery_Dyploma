using Shared.Primitives;

namespace RestaurantService.Domain.Events;

public sealed class RestaurantUpdatedDomainEvent : DomainEvent
{
    public Guid RestaurantId { get; }
    public string Name { get; }

    public RestaurantUpdatedDomainEvent(Guid restaurantId, string name)
    {
        RestaurantId = restaurantId;
        Name = name;
    }
}
