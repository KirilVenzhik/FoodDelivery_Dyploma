namespace RestaurantService.Domain.Exceptions;

public sealed class InvalidMenuItemPriceException : Exception
{
    public InvalidMenuItemPriceException(decimal price)
        : base($"Price '{price}' is invalid. Price must be greater than zero.") { }
}
