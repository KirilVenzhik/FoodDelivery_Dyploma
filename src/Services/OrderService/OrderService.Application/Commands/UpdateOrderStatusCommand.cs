using OrderService.Application.Common;
using OrderService.Application.Interfaces;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.Commands;

public record UpdateOrderStatusCommand(Guid OrderId, string NewStatus);

public sealed class UpdateOrderStatusCommandHandler
{
    private readonly IOrderRepository _repository;

    public UpdateOrderStatusCommandHandler(IOrderRepository repository)
        => _repository = repository;

    public async Task<CommandResult> HandleAsync(UpdateOrderStatusCommand command, CancellationToken ct = default)
    {
        var order = await _repository.GetByIdAsync(command.OrderId, ct);
        if (order is null)
            return CommandResult.NotFound();

        try
        {
            switch (command.NewStatus)
            {
                case "InPreparation":
                    order.StartPreparation();
                    break;
                case "ReadyForDelivery":
                    order.MarkReadyForDelivery();
                    break;
                default:
                    return CommandResult.UnprocessableEntity(
                        $"'{command.NewStatus}' is not a valid status for a RestaurantManager. Allowed: InPreparation, ReadyForDelivery.");
            }
        }
        catch (OrderDomainException ex)
        {
            return CommandResult.UnprocessableEntity(ex.Message);
        }

        await _repository.SaveChangesAsync(ct);
        return CommandResult.Success();
    }
}
