using RestaurantService.Application.DTOs;
using RestaurantService.Application.Interfaces;

namespace RestaurantService.Application.Queries;

public record GetMyRestaurantsQuery(Guid OwnerId);

public sealed class GetMyRestaurantsQueryHandler
{
    private readonly IRestaurantRepository _repository;

    public GetMyRestaurantsQueryHandler(IRestaurantRepository repository)
        => _repository = repository;

    public async Task<List<RestaurantSummaryResponse>> HandleAsync(
        GetMyRestaurantsQuery query,
        CancellationToken ct = default)
    {
        var restaurants = await _repository.GetByOwnerIdAsync(query.OwnerId, ct);
        return restaurants.Select(RestaurantSummaryResponse.FromDomain).ToList();
    }
}
