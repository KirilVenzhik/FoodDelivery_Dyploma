namespace OrderService.Application.DTOs;

public sealed class MenuItemDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;

    public decimal Price { get; set; }

    public bool IsAvailable { get; set; }
}
