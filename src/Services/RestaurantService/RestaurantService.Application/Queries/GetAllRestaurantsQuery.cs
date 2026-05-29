using RestaurantService.Application.DTOs;
using RestaurantService.Application.Interfaces;

namespace RestaurantService.Application.Queries;

public sealed class GetAllRestaurantsQuery { }

public sealed class GetAllRestaurantsQueryHandler
{
    private readonly IRestaurantRepository _repository;

    public GetAllRestaurantsQueryHandler(IRestaurantRepository repository)
        => _repository = repository;

    public async Task<List<RestaurantSummaryResponse>> HandleAsync(
        GetAllRestaurantsQuery query,
        CancellationToken ct = default)
    {
        var restaurants = await _repository.GetAllActiveAsync(ct);
        return restaurants.Select(RestaurantSummaryResponse.FromDomain).ToList();
    }
}
