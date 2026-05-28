namespace OrderService.Domain.Enums;

public enum OrderStatus
{
    PendingPayment = 0,
    Paid = 1,
    InPreparation = 2,
    ReadyForDelivery = 3,
    InDelivery = 4,
    Delivered = 5,
    PaymentFailed = 6
}
