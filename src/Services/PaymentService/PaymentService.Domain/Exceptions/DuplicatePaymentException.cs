namespace PaymentService.Domain.Exceptions;

public sealed class DuplicatePaymentException : Exception
{
    public DuplicatePaymentException(Guid orderId)
        : base($"Payment for order '{orderId}' has already been processed.") { }
}
