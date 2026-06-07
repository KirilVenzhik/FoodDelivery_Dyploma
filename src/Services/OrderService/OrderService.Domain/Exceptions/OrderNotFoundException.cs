namespace OrderService.Domain.Exceptions;

public sealed class OrderNotFoundException : Exception
{
    public OrderNotFoundException(Guid orderId)
        : base($"Order '{orderId}' was not found.") { }

    public OrderNotFoundException(string message)
        : base(message) { }
}
