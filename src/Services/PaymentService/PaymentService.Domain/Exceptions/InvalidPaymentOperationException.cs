namespace PaymentService.Domain.Exceptions;

public sealed class InvalidPaymentOperationException : Exception
{
    public InvalidPaymentOperationException(string message)
        : base(message) { }
}
