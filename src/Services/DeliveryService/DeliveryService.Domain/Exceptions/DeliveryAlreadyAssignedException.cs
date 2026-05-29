namespace DeliveryService.Domain.Exceptions;

public sealed class DeliveryAlreadyAssignedException : Exception
{
    public DeliveryAlreadyAssignedException(Guid id)
        : base($"Delivery '{id}' is already assigned and cannot be reassigned.") { }
}
