using OrderService.Domain.Entities;

namespace OrderService.Application.Interfaces;

public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken cancellationToken = default);

    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<Order>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default);

    Task<List<Order>> GetByRestaurantIdAsync(Guid restaurantId, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
