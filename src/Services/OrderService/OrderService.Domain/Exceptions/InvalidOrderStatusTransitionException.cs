using OrderService.Domain.Enums;

namespace OrderService.Domain.Exceptions;

public sealed class InvalidOrderStatusTransitionException : OrderDomainException
{
    public InvalidOrderStatusTransitionException(OrderStatus from, OrderStatus to)
        : base($"Cannot transition order from '{from}' to '{to}'.") { }

    public InvalidOrderStatusTransitionException(string message)
        : base(message) { }
}
