using Shared.Primitives;

namespace RestaurantService.Domain.Entities;

public class MenuItem : Entity
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = default!;

    public decimal Price { get; private set; }

    public bool IsAvailable { get; private set; }

    public Guid RestaurantId { get; private set; }

    private MenuItem()
    {
    }

    public static MenuItem Create(
        string name,
        decimal price)
    {
        return new MenuItem
        {
            Id = Guid.NewGuid(),
            Name = name,
            Price = price,
            IsAvailable = true
        };
    }

    public void Update(string name, decimal price)
    {
        Name = name;
        Price = price;
    }

    public void SetAvailability(bool available)
    {
        IsAvailable = available;
    }
}