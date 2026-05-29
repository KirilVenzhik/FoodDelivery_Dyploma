using RestaurantService.Domain.Entities;

namespace RestaurantService.Application.DTOs;

public sealed class MenuItemResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal Price { get; set; }
    public bool Available { get; set; }

    public static MenuItemResponse FromDomain(MenuItem m) => new()
    {
        Id = m.Id,
        Name = m.Name,
        Description = m.Description,
        Price = m.Price,
        Available = m.Available
    };
}
