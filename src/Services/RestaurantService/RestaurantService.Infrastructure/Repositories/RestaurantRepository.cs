using Microsoft.EntityFrameworkCore;
using RestaurantService.Application.Interfaces;
using RestaurantService.Domain.Entities;
using RestaurantService.Infrastructure.Persistence;

namespace RestaurantService.Infrastructure.Repositories;

public class RestaurantRepository
    : IRestaurantRepository
{
    private readonly RestaurantDbContext _dbContext;

    public RestaurantRepository(
        RestaurantDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        Restaurant restaurant,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Restaurants.AddAsync(
            restaurant,
            cancellationToken);
    }

    public async Task<Restaurant?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Restaurants
            .Include(x => x.MenuItems)
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<Restaurant?> GetByOwnerIdAsync(
        Guid ownerUserId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Restaurants
            .Include(x => x.MenuItems)
            .FirstOrDefaultAsync(
                x => x.OwnerUserId == ownerUserId,
                cancellationToken);
    }

    public async Task<List<Restaurant>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Restaurants
            .Include(x => x.MenuItems)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}