using RestaurantService.Application.DTOs;
using RestaurantService.Application.Interfaces;

namespace RestaurantService.Application.Queries;

public record GetRestaurantByIdQuery(Guid RestaurantId);

public sealed class GetRestaurantByIdQueryHandler
{
    private readonly IRestaurantRepository _repository;

    public GetRestaurantByIdQueryHandler(IRestaurantRepository repository)
        => _repository = repository;

    public async Task<RestaurantResponse?> HandleAsync(GetRestaurantByIdQuery query, CancellationToken ct = default)
    {
        var restaurant = await _repository.GetByIdAsync(query.RestaurantId, ct);
        return restaurant is null ? null : RestaurantResponse.FromDomain(restaurant);
    }
}
