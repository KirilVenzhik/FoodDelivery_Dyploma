namespace RestaurantService.Application.DTOs;

public sealed class CreateRestaurantRequest
{
    public string Name { get; set; } = default!;
}