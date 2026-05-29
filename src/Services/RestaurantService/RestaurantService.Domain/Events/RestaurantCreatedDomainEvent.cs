using Shared.Primitives;

namespace RestaurantService.Domain.Events;

public sealed class RestaurantCreatedDomainEvent : DomainEvent
{
    public Guid RestaurantId { get; }
    public Guid OwnerId { get; }
    public string Name { get; }

    public RestaurantCreatedDomainEvent(Guid restaurantId, Guid ownerId, string name)
    {
        RestaurantId = restaurantId;
        OwnerId = ownerId;
        Name = name;
    }
}
