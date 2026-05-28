using RestaurantService.Application.Common;
using RestaurantService.Application.Interfaces;

namespace RestaurantService.Application.Commands;

public record DeleteMenuItemCommand(Guid RestaurantId, Guid MenuItemId, Guid OwnerId);

public sealed class DeleteMenuItemCommandHandler
{
    private readonly IRestaurantRepository _repository;

    public DeleteMenuItemCommandHandler(IRestaurantRepository repository)
        => _repository = repository;

    public async Task<CommandResult> HandleAsync(DeleteMenuItemCommand command, CancellationToken ct = default)
    {
        var restaurant = await _repository.GetByIdAsync(command.RestaurantId, ct);
        if (restaurant is null)
            return CommandResult.NotFound();

        if (restaurant.OwnerUserId != command.OwnerId)
            return CommandResult.Forbidden();

        if (!restaurant.RemoveMenuItem(command.MenuItemId))
            return CommandResult.NotFound();

        await _repository.SaveChangesAsync(ct);

        return CommandResult.Success();
    }
}
