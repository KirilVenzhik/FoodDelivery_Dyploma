namespace PaymentService.Domain.Exceptions;

public sealed class PaymentNotFoundException : Exception
{
    public PaymentNotFoundException(Guid orderId)
        : base($"Payment for order '{orderId}' was not found.") { }
}
