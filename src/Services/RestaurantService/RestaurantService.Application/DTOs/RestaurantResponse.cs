using RestaurantService.Domain.Entities;

namespace RestaurantService.Application.DTOs;

public sealed class RestaurantResponse
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string Address { get; set; } = default!;
    public bool IsActive { get; set; }
    public string Status { get; set; } = default!;
    public List<MenuItemResponse> MenuItems { get; set; } = [];

    public static RestaurantResponse FromDomain(Restaurant r) => new()
    {
        Id = r.Id,
        OwnerId = r.OwnerId,
        Name = r.Name,
        Description = r.Description,
        Address = r.Address,
        IsActive = r.IsActive,
        Status = r.Status.ToString(),
        MenuItems = r.MenuItems.Select(MenuItemResponse.FromDomain).ToList()
    };
}
