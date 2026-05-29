using RestaurantService.Application.DTOs;
using RestaurantService.Application.Interfaces;
using RestaurantService.Domain.Exceptions;

namespace RestaurantService.Application.Commands;

public record ChangeRestaurantAvailabilityCommand(
    Guid RestaurantId,
    Guid RequestingOwnerId,
    bool IsActive);

public sealed class ChangeRestaurantAvailabilityCommandHandler
{
    private readonly IRestaurantRepository _repository;

    public ChangeRestaurantAvailabilityCommandHandler(IRestaurantRepository repository)
        => _repository = repository;

    public async Task<RestaurantResponse> HandleAsync(
        ChangeRestaurantAvailabilityCommand command,
        CancellationToken ct = default)
    {
        var restaurant = await _repository.GetByIdAsync(command.RestaurantId, ct)
            ?? throw new RestaurantNotFoundException(command.RestaurantId);

        if (restaurant.OwnerId != command.RequestingOwnerId)
            throw new RestaurantOwnershipException(command.RestaurantId, command.RequestingOwnerId);

        restaurant.ChangeAvailability(command.IsActive);
        await _repository.SaveChangesAsync(ct);

        return RestaurantResponse.FromDomain(restaurant);
    }
}
