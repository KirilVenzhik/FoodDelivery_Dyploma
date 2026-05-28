using RestaurantService.Domain.Entities;

namespace RestaurantService.Application.DTOs;

public sealed class RestaurantResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Status { get; set; } = default!;
    public List<MenuItemResponse> MenuItems { get; set; } = [];

    public static RestaurantResponse FromDomain(Restaurant r) => new()
    {
        Id = r.Id,
        Name = r.Name,
        Status = r.Status.ToString(),
        MenuItems = r.MenuItems.Select(MenuItemResponse.FromDomain).ToList()
    };
}
