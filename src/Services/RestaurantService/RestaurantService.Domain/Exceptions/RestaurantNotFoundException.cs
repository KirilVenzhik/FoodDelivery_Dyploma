namespace RestaurantService.Domain.Exceptions;

public sealed class RestaurantNotFoundException : Exception
{
    public RestaurantNotFoundException(Guid restaurantId)
        : base($"Restaurant '{restaurantId}' was not found.") { }
}
