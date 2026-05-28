namespace OrderService.Application.DTOs;

public sealed class UpdateOrderStatusRequest
{
    public string Status { get; set; } = default!;
}
