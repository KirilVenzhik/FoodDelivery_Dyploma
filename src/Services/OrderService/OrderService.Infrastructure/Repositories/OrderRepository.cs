using Microsoft.EntityFrameworkCore;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _dbContext;

    public OrderRepository(OrderDbContext dbContext)
        => _dbContext = dbContext;

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
        => await _dbContext.Orders.AddAsync(order, cancellationToken);

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbContext.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task<List<Order>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default)
        => await _dbContext.Orders
            .Include(o => o.Items)
            .Where(o => o.ClientId == clientId)
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<List<Order>> GetByRestaurantIdAsync(Guid restaurantId, CancellationToken ct = default)
        => await _dbContext.Orders
            .Include(o => o.Items)
            .Where(o => o.RestaurantId == restaurantId)
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync(ct);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _dbContext.SaveChangesAsync(cancellationToken);
}
