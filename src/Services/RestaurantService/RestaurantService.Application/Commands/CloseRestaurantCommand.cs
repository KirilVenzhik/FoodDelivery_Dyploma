using RestaurantService.Application.Common;
using RestaurantService.Application.Interfaces;

namespace RestaurantService.Application.Commands;

public record CloseRestaurantCommand(Guid RestaurantId, Guid OwnerId);

public sealed class CloseRestaurantCommandHandler
{
    private readonly IRestaurantRepository _repository;

    public CloseRestaurantCommandHandler(IRestaurantRepository repository)
        => _repository = repository;

    public async Task<CommandResult> HandleAsync(CloseRestaurantCommand command, CancellationToken ct = default)
    {
        var restaurant = await _repository.GetByIdAsync(command.RestaurantId, ct);
        if (restaurant is null)
            return CommandResult.NotFound();

        if (restaurant.OwnerUserId != command.OwnerId)
            return CommandResult.Forbidden();

        restaurant.Close();
        await _repository.SaveChangesAsync(ct);

        return CommandResult.Success();
    }
}
