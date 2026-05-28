using OrderService.Domain.Entities;
using Shared.Primitives;

namespace OrderService.Application.Interfaces;

public interface IOrderRepository
{
    Task AddAsync(
        Order order,
        CancellationToken cancellationToken = default);

    Task AddOutboxMessageAsync(
        OutboxMessage message,
        CancellationToken cancellationToken = default);

    Task<Order?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<List<Order>> GetByClientIdAsync(
        Guid clientId,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
