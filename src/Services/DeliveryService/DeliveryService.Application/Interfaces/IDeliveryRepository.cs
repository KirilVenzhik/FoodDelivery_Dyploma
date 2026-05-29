using DeliveryService.Domain.Entities;

namespace DeliveryService.Application.Interfaces;

public interface IDeliveryRepository
{
    Task AddAsync(Delivery delivery, CancellationToken cancellationToken = default);

    Task<Delivery?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Delivery?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);

    Task<List<Delivery>> GetByCourierIdAsync(Guid courierId, CancellationToken cancellationToken = default);

    Task<IEnumerable<Delivery>> GetPendingAsync(CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
