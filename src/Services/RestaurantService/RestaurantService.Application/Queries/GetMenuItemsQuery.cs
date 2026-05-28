using RestaurantService.Application.DTOs;
using RestaurantService.Application.Interfaces;

namespace RestaurantService.Application.Queries;

public record GetMenuItemsQuery(Guid RestaurantId);

public sealed class GetMenuItemsQueryHandler
{
    private readonly IRestaurantRepository _repository;

    public GetMenuItemsQueryHandler(IRestaurantRepository repository)
        => _repository = repository;

    public async Task<List<MenuItemResponse>?> HandleAsync(GetMenuItemsQuery query, CancellationToken ct = default)
    {
        var restaurant = await _repository.GetByIdAsync(query.RestaurantId, ct);
        if (restaurant is null) return null;

        return restaurant.MenuItems.Select(MenuItemResponse.FromDomain).ToList();
    }
}
