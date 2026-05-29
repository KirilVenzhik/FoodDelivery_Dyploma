using RestaurantService.Domain.Exceptions;
using Shared.Primitives;

namespace RestaurantService.Domain.Entities;

public class MenuItem : Entity
{
    public Guid Id { get; private set; }
    public Guid RestaurantId { get; private set; }
    public string Name { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public decimal Price { get; private set; }
    public bool Available { get; private set; }

    private MenuItem() { }

    public static MenuItem Create(
        Guid restaurantId,
        string name,
        string description,
        decimal price)
    {
        if (price <= 0)
            throw new InvalidMenuItemPriceException(price);

        return new MenuItem
        {
            Id = Guid.NewGuid(),
            RestaurantId = restaurantId,
            Name = name,
            Description = description,
            Price = price,
            Available = true
        };
    }

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice <= 0)
            throw new InvalidMenuItemPriceException(newPrice);
        Price = newPrice;
    }

    public void SetAvailability(bool available)
    {
        Available = available;
    }

    public void UpdateDescription(string description)
    {
        Description = description;
    }

    internal void UpdateName(string name)
    {
        Name = name;
    }
}
