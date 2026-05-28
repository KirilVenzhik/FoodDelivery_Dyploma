using RestaurantService.Application.DTOs;
using RestaurantService.Application.Interfaces;

namespace RestaurantService.Application.Queries;

public sealed class GetAllRestaurantsQueryHandler
{
    private readonly IRestaurantRepository _repository;

    public GetAllRestaurantsQueryHandler(IRestaurantRepository repository)
        => _repository = repository;

    public async Task<List<RestaurantResponse>> HandleAsync(CancellationToken ct = default)
    {
        var restaurants = await _repository.GetAllAsync(ct);
        return restaurants.Select(RestaurantResponse.FromDomain).ToList();
    }
}
