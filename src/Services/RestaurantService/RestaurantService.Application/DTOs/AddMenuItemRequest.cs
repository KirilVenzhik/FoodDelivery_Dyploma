namespace RestaurantService.Application.DTOs;

public sealed class AddMenuItemRequest
{
    public string Name { get; set; } = default!;

    public decimal Price { get; set; }
}