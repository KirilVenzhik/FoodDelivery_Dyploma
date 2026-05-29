using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.Mapping;
using OrderService.Domain.Enums;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.Commands;

public record ChangeOrderStatusCommand(Guid OrderId, string NewStatus);

public sealed class ChangeOrderStatusCommandHandler
{
    private readonly IOrderRepository _repository;

    public ChangeOrderStatusCommandHandler(IOrderRepository repository)
        => _repository = repository;

    public async Task<OrderResponse> HandleAsync(ChangeOrderStatusCommand command, CancellationToken ct = default)
    {
        var order = await _repository.GetByIdAsync(command.OrderId, ct)
            ?? throw new OrderNotFoundException(command.OrderId);

        if (!Enum.TryParse<OrderStatus>(command.NewStatus, ignoreCase: true, out var newStatus))
            throw new InvalidOrderStatusTransitionException(
                $"'{command.NewStatus}' is not a valid order status.");

        order.ChangeStatus(newStatus);

        await _repository.SaveChangesAsync(ct);

        return OrderMapper.MapToResponse(order);
    }
}
