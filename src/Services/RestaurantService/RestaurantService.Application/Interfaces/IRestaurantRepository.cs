using RestaurantService.Domain.Entities;

namespace RestaurantService.Application.Interfaces;

public interface IRestaurantRepository
{
    Task AddAsync(
        Restaurant restaurant,
        CancellationToken cancellationToken = default);

    Task<Restaurant?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Restaurant?> GetByOwnerIdAsync(
        Guid ownerUserId,
        CancellationToken cancellationToken = default);

    Task<List<Restaurant>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}