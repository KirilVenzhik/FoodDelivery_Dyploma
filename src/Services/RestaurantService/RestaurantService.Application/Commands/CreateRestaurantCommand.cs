using RestaurantService.Application.Common;
using RestaurantService.Application.Interfaces;
using RestaurantService.Domain.Entities;

namespace RestaurantService.Application.Commands;

public record CreateRestaurantCommand(Guid OwnerId, string Name);

public sealed class CreateRestaurantCommandHandler
{
    private readonly IRestaurantRepository _repository;

    public CreateRestaurantCommandHandler(IRestaurantRepository repository)
        => _repository = repository;

    public async Task<CommandResult> HandleAsync(CreateRestaurantCommand command, CancellationToken ct = default)
    {
        var existing = await _repository.GetByOwnerIdAsync(command.OwnerId, ct);
        if (existing is not null)
            return CommandResult.Conflict();

        var restaurant = Restaurant.Create(command.OwnerId, command.Name);
        await _repository.AddAsync(restaurant, ct);
        await _repository.SaveChangesAsync(ct);

        return CommandResult.Success(restaurant.Id);
    }
}
