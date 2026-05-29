namespace RestaurantService.Application.DTOs;

public sealed class UpdateMenuItemRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public bool? Available { get; set; }
}
