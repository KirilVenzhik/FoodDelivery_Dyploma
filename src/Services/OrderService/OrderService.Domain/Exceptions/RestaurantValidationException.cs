namespace OrderService.Domain.Exceptions;

public sealed class RestaurantValidationException : Exception
{
    public RestaurantValidationException(string message) : base(message) { }
}
