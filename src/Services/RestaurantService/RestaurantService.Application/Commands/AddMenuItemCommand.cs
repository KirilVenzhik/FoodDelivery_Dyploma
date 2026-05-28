using RestaurantService.Application.Common;
using RestaurantService.Application.Interfaces;

namespace RestaurantService.Application.Commands;

public record AddMenuItemCommand(Guid RestaurantId, Guid OwnerId, string Name, decimal Price);

public sealed class AddMenuItemCommandHandler
{
    private readonly IRestaurantRepository _repository;

    public AddMenuItemCommandHandler(IRestaurantRepository repository)
        => _repository = repository;

    public async Task<CommandResult> HandleAsync(AddMenuItemCommand command, CancellationToken ct = default)
    {
        var restaurant = await _repository.GetByIdAsync(command.RestaurantId, ct);
        if (restaurant is null)
            return CommandResult.NotFound();

        if (restaurant.OwnerUserId != command.OwnerId)
            return CommandResult.Forbidden();

        var item = restaurant.AddMenuItem(command.Name, command.Price);
        await _repository.SaveChangesAsync(ct);

        return CommandResult.Success(item.Id);
    }
}
