namespace OrderService.Domain.Exceptions;

public sealed class OrderItemsEmptyException : OrderDomainException
{
    public OrderItemsEmptyException()
        : base("Order must contain at least one item.") { }
}
