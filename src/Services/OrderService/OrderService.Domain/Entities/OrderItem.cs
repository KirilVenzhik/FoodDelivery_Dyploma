using Shared.Primitives;

namespace OrderService.Domain.Entities;

public class OrderItem : Entity
{
    public Guid Id { get; private set; }

    public Guid OrderId { get; private set; }

    public Guid MenuItemId { get; private set; }

    public string ItemName { get; private set; } = default!;

    public decimal UnitPrice { get; private set; }

    public int Quantity { get; private set; }

    public decimal LineTotal { get; private set; }

    private OrderItem() { }

    public static OrderItem Create(
        Guid menuItemId,
        string itemName,
        decimal unitPrice,
        int quantity)
    {
        return new OrderItem
        {
            Id = Guid.NewGuid(),
            MenuItemId = menuItemId,
            ItemName = itemName,
            UnitPrice = unitPrice,
            Quantity = quantity,
            LineTotal = unitPrice * quantity
        };
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(newQuantity), "Quantity must be positive.");

        Quantity = newQuantity;
        LineTotal = UnitPrice * newQuantity;
    }
}
