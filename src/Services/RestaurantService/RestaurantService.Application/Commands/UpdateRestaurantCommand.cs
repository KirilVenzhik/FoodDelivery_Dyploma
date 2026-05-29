using RestaurantService.Application.DTOs;
using RestaurantService.Application.Interfaces;
using RestaurantService.Domain.Exceptions;

namespace RestaurantService.Application.Commands;

public record UpdateRestaurantCommand(
    Guid RestaurantId,
    Guid RequestingOwnerId,
    string Name,
    string Description,
    string Address);

public sealed class UpdateRestaurantCommandHandler
{
    private readonly IRestaurantRepository _repository;

    public UpdateRestaurantCommandHandler(IRestaurantRepository repository)
        => _repository = repository;

    public async Task<RestaurantResponse> HandleAsync(
        UpdateRestaurantCommand command,
        CancellationToken ct = default)
    {
        var restaurant = await _repository.GetByIdAsync(command.RestaurantId, ct)
            ?? throw new RestaurantNotFoundException(command.RestaurantId);

        if (restaurant.OwnerId != command.RequestingOwnerId)
            throw new RestaurantOwnershipException(command.RestaurantId, command.RequestingOwnerId);

        restaurant.Update(command.Name, command.Description, command.Address);
        await _repository.SaveChangesAsync(ct);

        return RestaurantResponse.FromDomain(restaurant);
    }
}
