namespace NotificationService.Domain.ValueObjects;

public static class NotificationMessage
{
    public static string ForOrderPlaced(Guid orderId) =>
        $"Your order {orderId} has been placed and payment is being processed.";

    public static string ForPaymentSuccess(Guid orderId) =>
        $"Payment for order {orderId} was successful. Your order is being prepared.";

    public static string ForPaymentFailed(Guid orderId) =>
        $"Payment for order {orderId} failed. Your order has been cancelled.";

    public static string ForOrderDelivered(Guid orderId) =>
        $"Your order {orderId} has been delivered. Enjoy your meal!";
}
