using RestaurantService.Domain.Entities;

namespace RestaurantService.Application.Interfaces;

public interface IRestaurantRepository
{
    Task AddAsync(Restaurant restaurant, CancellationToken cancellationToken = default);
    Task AddMenuItemAsync(MenuItem menuItem, CancellationToken cancellationToken = default);
    Task<Restaurant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Restaurant?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Restaurant>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Restaurant>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task UpdateAsync(Restaurant restaurant, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
