using RestaurantService.Application.Interfaces;
using RestaurantService.Domain.Exceptions;

namespace RestaurantService.Application.Commands;

public record RemoveMenuItemCommand(
    Guid RestaurantId,
    Guid MenuItemId,
    Guid RequestingOwnerId);

public sealed class RemoveMenuItemCommandHandler
{
    private readonly IRestaurantRepository _repository;

    public RemoveMenuItemCommandHandler(IRestaurantRepository repository)
        => _repository = repository;

    public async Task HandleAsync(
        RemoveMenuItemCommand command,
        CancellationToken ct = default)
    {
        var restaurant = await _repository.GetByIdWithItemsAsync(command.RestaurantId, ct)
            ?? throw new RestaurantNotFoundException(command.RestaurantId);

        if (restaurant.OwnerId != command.RequestingOwnerId)
            throw new RestaurantOwnershipException(command.RestaurantId, command.RequestingOwnerId);

        restaurant.RemoveMenuItem(command.MenuItemId);
        await _repository.SaveChangesAsync(ct);
    }
}
