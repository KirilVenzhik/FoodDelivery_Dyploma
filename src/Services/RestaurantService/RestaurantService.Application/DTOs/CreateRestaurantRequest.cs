namespace RestaurantService.Application.DTOs;

public sealed class CreateRestaurantRequest
{
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string Address { get; set; } = default!;
}
