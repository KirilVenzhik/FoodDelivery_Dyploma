namespace RestaurantService.Domain.Exceptions;

public sealed class MenuItemNotFoundException : Exception
{
    public MenuItemNotFoundException(Guid menuItemId)
        : base($"Menu item '{menuItemId}' was not found.") { }
}
