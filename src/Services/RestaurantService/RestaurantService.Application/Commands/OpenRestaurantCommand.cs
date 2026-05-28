using RestaurantService.Application.Common;
using RestaurantService.Application.Interfaces;

namespace RestaurantService.Application.Commands;

public record OpenRestaurantCommand(Guid RestaurantId, Guid OwnerId);

public sealed class OpenRestaurantCommandHandler
{
    private readonly IRestaurantRepository _repository;

    public OpenRestaurantCommandHandler(IRestaurantRepository repository)
        => _repository = repository;

    public async Task<CommandResult> HandleAsync(OpenRestaurantCommand command, CancellationToken ct = default)
    {
        var restaurant = await _repository.GetByIdAsync(command.RestaurantId, ct);
        if (restaurant is null)
            return CommandResult.NotFound();

        if (restaurant.OwnerUserId != command.OwnerId)
            return CommandResult.Forbidden();

        restaurant.Open();
        await _repository.SaveChangesAsync(ct);

        return CommandResult.Success();
    }
}
