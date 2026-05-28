using DeliveryService.Application.Interfaces;
using DeliveryService.Domain.Entities;
using DeliveryService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DeliveryService.Infrastructure.Repositories;

public class DeliveryRepository : IDeliveryRepository
{
    private readonly DeliveryDbContext _dbContext;

    public DeliveryRepository(DeliveryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        Delivery delivery,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Deliveries.AddAsync(delivery, cancellationToken);
    }

    public async Task<Delivery?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Deliveries
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<Delivery?> GetByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Deliveries
            .FirstOrDefaultAsync(d => d.OrderId == orderId, cancellationToken);
    }

    public async Task<List<Delivery>> GetByCourierIdAsync(
        Guid courierId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Deliveries
            .Where(d => d.CourierId == courierId)
            .OrderByDescending(d => d.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
