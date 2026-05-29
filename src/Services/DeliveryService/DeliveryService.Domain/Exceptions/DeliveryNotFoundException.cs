namespace DeliveryService.Domain.Exceptions;

public sealed class DeliveryNotFoundException : Exception
{
    public DeliveryNotFoundException(Guid id)
        : base($"Delivery with id '{id}' was not found.") { }
}
