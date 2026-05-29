using DeliveryService.Domain.Enums;

namespace DeliveryService.Domain.Exceptions;

public sealed class InvalidDeliveryStatusTransitionException : Exception
{
    public InvalidDeliveryStatusTransitionException(DeliveryStatus current, DeliveryStatus target)
        : base($"Cannot transition delivery from '{current}' to '{target}'.") { }
}
