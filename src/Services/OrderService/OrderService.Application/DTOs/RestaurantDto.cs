namespace OrderService.Application.DTOs;

public sealed class RestaurantDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;

    public string Status { get; set; } = default!;

    public List<MenuItemDto> MenuItems { get; set; } = [];

    public bool IsActive => Status == "Open";
}
