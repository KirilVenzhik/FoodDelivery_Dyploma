namespace RestaurantService.Application.DTOs;

public sealed class UpdateMenuItemRequest
{
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
}
