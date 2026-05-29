using RestaurantService.Application.DTOs;
using RestaurantService.Application.Interfaces;
using RestaurantService.Domain.Exceptions;

namespace RestaurantService.Application.Queries;

public record GetRestaurantByIdQuery(Guid RestaurantId);

public sealed class GetRestaurantByIdQueryHandler
{
    private readonly IRestaurantRepository _repository;

    public GetRestaurantByIdQueryHandler(IRestaurantRepository repository)
        => _repository = repository;

    public async Task<RestaurantResponse> HandleAsync(
        GetRestaurantByIdQuery query,
        CancellationToken ct = default)
    {
        var restaurant = await _repository.GetByIdWithItemsAsync(query.RestaurantId, ct)
            ?? throw new RestaurantNotFoundException(query.RestaurantId);

        return RestaurantResponse.FromDomain(restaurant);
    }
}
