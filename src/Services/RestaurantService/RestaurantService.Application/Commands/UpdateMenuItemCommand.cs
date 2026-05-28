using RestaurantService.Application.Common;
using RestaurantService.Application.Interfaces;

namespace RestaurantService.Application.Commands;

public record UpdateMenuItemCommand(Guid RestaurantId, Guid MenuItemId, Guid OwnerId, string Name, decimal Price);

public sealed class UpdateMenuItemCommandHandler
{
    private readonly IRestaurantRepository _repository;

    public UpdateMenuItemCommandHandler(IRestaurantRepository repository)
        => _repository = repository;

    public async Task<CommandResult> HandleAsync(UpdateMenuItemCommand command, CancellationToken ct = default)
    {
        var restaurant = await _repository.GetByIdAsync(command.RestaurantId, ct);
        if (restaurant is null)
            return CommandResult.NotFound();

        if (restaurant.OwnerUserId != command.OwnerId)
            return CommandResult.Forbidden();

        var item = restaurant.MenuItems.FirstOrDefault(m => m.Id == command.MenuItemId);
        if (item is null)
            return CommandResult.NotFound();

        item.Update(command.Name, command.Price);
        await _repository.SaveChangesAsync(ct);

        return CommandResult.Success();
    }
}
